# Testing

The tests are the specification of the mocks: the same test runs against the real implementation and the mock, so a passing real run proves the expectation and a passing mock run proves the mock matches it.

## Framework

- TUnit (`TUnit.Engine`) on Microsoft.Testing.Platform, aweXpect assertions (`await That(...)`, with `aweXpect.Testably` and `aweXpect.Chronology`), Mockolate for mocks and AutoFixture through `[AutoArguments]`.
- Tests are `async Task` methods with `[Test]`; data comes from `[Arguments(...)]`, `[AutoArguments]` or `[MethodDataSource]`. Prefer `[AutoArguments]` for paths and contents whose value does not matter.
- A module initializer (`TestCultureInitializer`) runs all tests under the invariant culture, so exception messages are stable on .NET Framework.

## Conventions

- Name tests `<Member>[_<Scenario>]_Should<Outcome>`, e.g. `Copy_SourceAccessedWithReadShare_ShouldNotThrow`.
- Separate arrange, act and assert with blank lines instead of comments. Name the object under test `sut` when it is not the `FileSystem` of the test base.
- Capture an expected exception with a local `void Act()` and `await That(Act).Throws…()`, or with `Record.Exception(...)` when the test also asserts the state afterwards.
- Put the reason an assertion holds into `.Because(...)` instead of into a comment.
- Test through observable behaviour (return values, exceptions, the file system state, the identity of a statistics entry), not through `ToString()` or other implementation details.

## File system tests

```csharp
[FileSystemTests]
public class ExistsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsFalse();
	}
}
```

- `[FileSystemTests]` creates one instance of the class per file system: `MockFileSystem`, mocks simulating Linux, macOS and Windows (not on `net48`), and `RealFileSystem`.
- `FileSystemTestBase` provides `FileSystem`, `TimeSystem`, `Test`, `BasePath` and `CancellationToken`. Each test starts in an empty temporary directory that is deleted afterwards. Real file system instances never run in parallel with each other, and every test is retried once (`[Retry(1)]`).
- Use relative paths or paths below `BasePath`. Never touch fixed locations on the real file system.
- A test that changes the current directory restores it in a `finally` block. The real file system tests share the process, and on Linux and macOS deleting the test directory succeeds even while it is the current directory, which breaks every later call that reads the current directory of the process.
- Time and random tests use `[TimeSystemTests]`/`TimeSystemTestBase` and `[RandomSystemTests]`/`RandomSystemTestBase` in the same way.
- Mock-only features (`Initialize()`, notifications, interception, statistics, `SimulationMode`) belong in `Tests/Testably.Abstractions.Testing.Tests` and use `MockFileSystem` directly.

## OS- and framework-specific behaviour

- `Test.RunsOnWindows`, `Test.RunsOnLinux`, `Test.RunsOnMac` and `Test.IsNetFramework` describe the platform of the current instance. For a simulated mock they report the simulated OS, not the host.
- Use `Skip.If(condition, reason)` and `Skip.IfNot(condition, reason)` (from the test helpers, not `TUnit.Core.Skip`) when an expectation only holds on some platforms, and write a separate test for the behaviour on the other platforms.
- Assert the exception type and its `HResult` (`Throws<IOException>().WithHResult(...)`); assert the message only where it is the same on every platform the test runs on.
- Code under test in the mock branches on `Execute.IsWindows` and friends; a missing OS branch shows up as a failing simulated-OS or real-OS run.

## Test settings

`Tests/Helpers/Testably.Abstractions.TestHelpers/Settings.cs` holds three switches with the values `AlwaysEnabled`, `DisabledInDebugMode` and `AlwaysDisabled`:

| Setting | Default | Effect |
|---|---|---|
| `RealFileSystemTests` | `DisabledInDebugMode` | In Debug the `RealFileSystem` instance is only created when set to `AlwaysEnabled`; in Release unless set to `AlwaysDisabled` |
| `LongRunningTests` | `DisabledInDebugMode` | Read by `SkipIfLongRunningTestsShouldBeSkipped()` |
| `BrittleTests` | `AlwaysDisabled` | Read by `SkipIfBrittleTestsShouldBeSkipped()` for timing-sensitive real-system tests |

To check a behaviour against the real file system locally, run in Release (`-c Release`) or change the default temporarily; do not commit that change. Real file system runs are slower and hit OS-specific behaviour, so filter them to the classes you changed.

## Parity tests

`Tests/Testably.Abstractions.Parity.Tests` compares each interface with its BCL counterpart by reflection. `ParityTests` holds the checks and `Net48ParityTests`, `Net8ParityTests`, `Net9ParityTests`, `Net10ParityTests` and `Net11ParityTests` activate them per framework. Members that are intentionally not mirrored are excluded in `TestHelpers/Parity.cs` (all frameworks) or in the constructor of a `Net*ParityTests` class (one framework, e.g. .NET Framework members missing from .NET Standard 2.0). When the BCL adds an API on a supported framework, add it to the interface behind a `FEATURE_*` flag rather than to the exclusion list, unless there is a reason not to mirror it.
