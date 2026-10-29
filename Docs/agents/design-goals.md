# Design goals

These goals guide agents and contributors when a change to Testably.Abstractions has more than one reasonable solution. They describe the rules the code base already follows; follow them in new and changed code.

Correctness is a hard requirement. Every other goal is a strong default: follow it, and when a change has to break one, say so in the pull request and explain why.

## Decision test

Before proposing a change, ask: does the mock still behave like the real system, does the public API stay minimal and compatible, and does the change make the library more consistent, easier to extend or more pleasant to use? If the answer is no, reconsider the change.

## Goals

When goals conflict, the earlier one wins.

1. **Behave like the real system (must).** On every simulated operating system (OS) and target framework, the real file system is the oracle. A mock returns the same results and fails with the same exception type, message and `HResult` as the real implementation on that platform. Differences are fixed in the mock, never in the test.
2. **Stable and compatible.** Follow semantic versioning: breaking changes only in a major version, on purpose and documented. The Core and main packages are versioned separately, and the main packages only see an interface change after a Core release. The `IFileSystem` interfaces in `Testably.Abstractions.FileSystem.Interface` are shared with [TestableIO.System.IO.Abstractions](https://github.com/TestableIO/System.IO.Abstractions), so their namespace (`System.IO.Abstractions`), assembly and shape are a contract with that ecosystem as well. Keep dependencies minimal and their version floors low.
3. **A minimal public API.** Every public member is a permanent commitment, so add one only for a concrete use case.
   - The interfaces mirror the API of the .NET base class library (BCL) types exactly, per target framework: no additional members, no missing ones. The parity tests enforce this; intentional gaps are excluded explicitly.
   - Anything beyond the BCL lives in the mock-only API of `Testably.Abstractions.Testing` or in a companion package, never in the interfaces.
   - Everything that is not meant for users is `internal`. Tests reach internals through `InternalsVisibleTo`, not through a public member.
4. **A consistent and pleasant mock-only API.** New features follow the existing patterns (see [Code](#code)), every public member has XML documentation, and defaults work without configuration.
5. **Extensible.** Third parties build on the same extension points the library uses itself: the companion packages reach the wrapped real object through `IFileSystemExtensibility`, and tests customise behaviour through strategies. Design an extension point so that it can grow without breaking implementers.
6. **Thread-safe mock state.** Tests run in parallel and production code under test may use several threads, so mock state is safe for concurrent access.
7. **Fast on the happy path.** The mocks run in large test suites. Avoid allocations and locks on hot paths such as path handling, storage access and statistics registration, and measure before claiming a performance improvement.
8. **Documented.** A public feature is not done until it is documented on the docs site (`Docs/pages/docs`), with examples that match the tested behaviour.

## Code

### Mirroring the BCL

- Each real type has a `*Wrapper` in `Testably.Abstractions` that forwards to the BCL and a `*Mock` in `Testably.Abstractions.Testing` that implements the behaviour in memory.
- Express API differences between target frameworks with a `FEATURE_*` constant in `Feature.Flags.props` instead of raw `NETx_0_OR_GREATER` checks, and use the same flag on the interface, the wrapper, the mock and the tests.
- Document an implemented interface member with `/// <inheritdoc cref="IFile.Exists(string)" />` (an explicit `cref`), and group the members of an implemented interface in `#region <Interface> Members`.

### Inside the mocks

- **Use the mock's own context, never the host.** Branch on the mock's `Execute` (`IsWindows`, `IsLinux`, `IsMac`, `IsNetFramework`) instead of the host OS, resolve paths with `Execute.Path` and the mock's current directory instead of `System.IO.Path`, and take timestamps from the mock's `TimeSystem` instead of `DateTime.Now`. A host API in the mock breaks the simulated operating systems and couples tests to the state of the test process.
- **Create exceptions in `ExceptionFactory`.** It produces the type, message and `HResult` of the real exception for the simulated OS and framework.
- **Register statistics.** Every public member of a mock starts with `using IDisposable registration = ...StatisticsRegistration...Register...(...)`, so that `MockFileSystem.Statistics` records the call.
- **Notify changes.** Changes to the storage go through `ChangeHandler.NotifyPendingChange` and `NotifyCompletedChange`, so that interception, notifications and the `FileSystemWatcherMock` observe them.
- **Keep state thread-safe** with `ConcurrentDictionary` or a `lock`, and keep the locked section short.

### The mock-only API

- **Configure the mock in two places.** Options passed to the constructor configure the system before it is created (`new MockFileSystem(o => o.SimulatingOperatingSystem(...).UseCurrentDirectory())`, with `Use…`, `Simulating…`, `Disable…` and `Without…` methods). `With…` methods configure an existing instance and return it for chaining (`WithDrive`, `WithAccessControlStrategy`, `WithTimerStrategy`).
- **Name the other entry points consistently.** `Initialize…` seeds files and directories, `Intercept` and `Notify` observe changes before and after they happen, and `Statistics` records calls.
- **Customise behaviour with a strategy.** A strategy is an `I…Strategy` interface, set through a `With…Strategy` method. Where it fits, ship a callback-based `Default…Strategy` and a `Null…Strategy` that does not restrict anything.
- **Keep types the mock creates closed.** A type that the mock creates and user code only reads (for example `ChangeDescription`) has get-only properties and an `internal` constructor, so that it can gain members later without breaking callers. Pass information to a public callback that may grow in such a request object instead of as separate parameters.
- **Seal new public classes** unless they are designed for inheritance; unsealing later is compatible, sealing is not.

### Language and frameworks

- Use current C# features where every target framework supports them. `netstandard2.0` has no polyfill package: hand-written polyfills live in `Source/Testably.Abstractions.Testing/Polyfills/` behind `#if`, and features that need runtime types (records, `init`, `Index`/`Range`) only compile behind a `FEATURE_*` or `NET*` guard. Test projects use the `Polyfill` package.
- Avoid reflection. Use it only where the real types offer no other way, such as setting the private fields of `FileSystemEventArgs`.
- **Async.**
  - Return types are dictated by the mirrored BCL API.
  - Pass the `CancellationToken` through every async path.
  - Use `ConfigureAwait(false)` on every `await` in library code, so that callers who block on a result cannot deadlock on a single-threaded synchronization context. Existing code does not follow this everywhere yet; new and changed code does.
  - Do not use `Task.Run` to make code look asynchronous; use it only where the real counterpart does work in the background.
- **Avoid.**
  - Catching exceptions without handling or rethrowing them.
  - LINQ, closures and boxing in hot paths; use loops and cached delegates there.

## Before you commit

Run the checks that match the change:

- **Any code change:** the tests of the affected projects on `net10.0`, and on `net48` when .NET Framework behaviour is involved.
- **Mock behaviour:** the same tests against the real file system (Release build, see [testing.md](testing.md#test-settings)); CI then covers Windows, Linux and macOS.
- **Public API:** the API snapshot tests; review and accept intentional changes, never commit pending snapshots.
- **New BCL API:** the parity tests on each affected framework.
- **New public feature:** documentation and its examples.
- **Static analysis:** no new Sonar or analyzer findings. Avoid the common ones: high cognitive complexity, duplicated string literals, unused members and parameters, methods that could be static, long parameter lists. Do not suppress a finding without asking the maintainer first.

## Reviews

Verify every review finding against the code before changing anything. If you disagree, push back once with concrete reasoning; if the reviewer or the maintainer confirms the finding, implement it.
