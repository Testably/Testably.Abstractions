# Design goals

These goals apply to every Testably repository. They guide agents and contributors when a change has more than one reasonable solution. The goals of this repository at the end refine them.

Correctness is a hard requirement. Every other goal is a strong default: follow it, and when a change has to break one, say so in the pull request and explain why.

## Decision test

Before proposing a change, ask: does it keep the library correct, and does it make it faster, more consistent, easier to extend or more pleasant to use? If the answer is no, reconsider the change.

## Goals

When goals conflict, the earlier one wins.

1. **Correct, with clear failures (must).** Behaviour is exact and covered by tests. A failure states what was expected and what was found, and tests pin the complete message.
2. **Stable and compatible.** Follow semantic versioning: breaking changes only in a major version, on purpose and documented. Public API changes show up in the API snapshots and are reviewed. Keep dependencies minimal and their version floors low, and support the declared target frameworks.
3. **AOT and trimming friendly.** No unguarded reflection: prefer source generators, otherwise put reflection behind a feature guard with trimming annotations. A change that touches reflection runs the AOT smoke tests.
4. **A consistent, discoverable and pleasant API.** Extension methods exist only on the types they apply to, so IntelliSense shows what makes sense. Naming, option chaining and null handling follow the existing patterns (see the repository-specific rules). Every public member has XML documentation. Prefer minimal ceremony for the user: sensible defaults, no boilerplate, failure messages that read well.
5. **Fast, especially on the happy path.** Succeeding calls are the common case. Avoid allocations in hot paths, and back every performance claim or change with a benchmark before and after.
6. **Extensible.** Third parties build on the same extension points the library uses itself. New features use the public extension surface instead of internals, and the core stays small and stable.
7. **Errors at compile time rather than at run time.** Prefer analyzers, source generators and types that make misuse a compiler error over runtime checks.
8. **Documented and pleasant to read.** A public feature is not done until it is documented on the docs site, with examples that are tested where the repository can test them. The docs are rendered with Docusaurus: use its features where they help the reader, such as admonitions (`:::note`, `:::tip`, `:::warning`) for remarks, tabs for alternatives (for example synchronous and asynchronous usage, or one tab per test framework) and collapsible `<details>` for long output, and follow the style of the existing pages.

## Code

- **Modern C#.** Use current language features and APIs where the target frameworks allow them, with polyfills for netstandard2.0 and net48 instead of avoiding a feature. Check that the oldest target framework still compiles.
- **Async.**
  - Use `ValueTask` for operations that often complete synchronously.
  - Pass the `CancellationToken` through every async path.
  - Use `ConfigureAwait(false)` on every `await` in library code, so that callers who block on a result cannot deadlock on a single-threaded synchronization context. Existing code does not follow this everywhere yet; new and changed code does.
  - Do not block on async code (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`), except at a deliberate synchronous API boundary, and complete synchronously there where possible.
  - Do not use `Task.Run` to make code look asynchronous; use it only where the real counterpart does work in the background.
- **Avoid.**
  - Catching exceptions without handling or rethrowing them.
  - LINQ, closures and boxing in hot paths; use loops and cached delegates there.
  - String concatenation in loops; use a `StringBuilder`.

## Before you commit

Run the checks that match the change:

- **Any code change:** the tests of the affected projects, on the oldest and the newest target framework.
- **Public API:** the API snapshot tests; review and accept intentional changes, never commit pending snapshots.
- **Reflection, trimming or AOT:** the AOT smoke tests.
- **Hot path or a performance claim:** a benchmark before and after.
- **New public feature:** documentation and its examples.
- **Failure messages:** tests that pin the complete message.
- **Static analysis:** no new Sonar or analyzer findings. Avoid the common ones: high cognitive complexity, duplicated string literals, unused members and parameters, methods that could be static, long parameter lists. Do not suppress a finding without asking the maintainer first.

## Reviews

Verify every review finding against the code before changing anything. If you disagree, push back once with concrete reasoning; if the reviewer or the maintainer confirms the finding, implement it.

## Goals of this repository

1. **Behaves like the real system.** On the simulated OS and target framework the real file system is the oracle; differences are fixed in the mock, never in the test.
2. **Interfaces mirror the BCL exactly** per target framework, checked by the parity tests.
3. **Mocks fail like the real system.** Same exception type, message and HResult per OS.
4. **OS behaviour through the mock's own context**, so simulated operating systems stay correct.
5. **Consistent mock-only API.** `With…` configures, `Initialize…` seeds, `Intercept`/`Notify` observe.
6. **Deliberate package evolution.** Interface changes reach the mocks only after a Core release.
7. **Thread-safe mock state.**
