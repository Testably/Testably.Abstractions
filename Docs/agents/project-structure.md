# Project structure

## Layout

| Path | Content |
|---|---|
| `Source/` | Shipped libraries; `Source/Directory.Build.props` sets target frameworks, packaging and analyzers |
| `Tests/` | Test projects, shared helpers in `Tests/Helpers/`, API snapshot tests in `Tests/Api/`; `Tests/Directory.Build.props` sets the test frameworks and packages |
| `Pipeline/` | Fallout build (`Build.*.cs` partial classes, one per target group) |
| `Docs/` | Published docs (`Docs/pages/docs`), package readmes (`Docs/Nuget`), maintainer notes, images, this folder |
| `.github/workflows/` | `ci.yml`, `build.yml`, `notify-docs-site.yml` |
| `Feature.Flags.props` | `FEATURE_*` compilation constants per target framework, imported by `Source/` and `Tests/` |
| `Directory.Packages.props` | Central package versions, including the version range of the Core packages consumed by the main packages |

## Packages

Core packages (released from `core/v*` tags, solution folder `/Core/`):

| Project | Content |
|---|---|
| `Testably.Abstractions.FileSystem.Interface` | `IFileSystem` and its parts (`IFile`, `IDirectory`, `IPath`, `IFileInfo`, `IFileSystemWatcher`, `IRandomAccess`, `FileSystemStream`, ...) in the `System.IO.Abstractions` namespace |
| `Testably.Abstractions.Interface` | `ITimeSystem` and `IRandomSystem` with their parts (`IDateTime`, `ITask`, `IThread`, `ITimer`, `IStopwatch`, `IRandom`, `IGuid`, ...); references the file system interface project |
| `Testably.Abstractions` | `RealFileSystem`, `RealTimeSystem`, `RealRandomSystem`: thin wrappers that forward to the BCL |

Main packages (released from `v*` tags), each referencing `Testably.Abstractions.Interface` as a NuGet package:

| Project | Content |
|---|---|
| `Testably.Abstractions.Testing` | `MockFileSystem`, `MockTimeSystem`, `MockRandomSystem` and their test helpers |
| `Testably.Abstractions.Compression` | `ZipFile`/`ZipArchive` abstractions as extension methods on `IFileSystem` |
| `Testably.Abstractions.AccessControl` | ACL extension methods for files, directories and streams |
| `Testably.Abstractions.MemoryMappedFiles` | `MemoryMappedFile` abstractions, with a real wrapper and a mock implementation |

The companion packages contain both the wrapper over the real API and the mock behaviour. They reach the underlying object through `IFileSystemExtensibility` (`TryGetWrappedInstance`): an entity that wraps a real `FileStream`, `FileInfo` and so on uses the real API, anything else gets the mock behaviour.

## Inside `Testably.Abstractions.Testing`

| Folder | Content |
|---|---|
| `FileSystem/` | `*Mock` implementations of every file system interface (`FileMock`, `DirectoryMock`, `FileStreamMock`, `PathMock`, `FileSystemWatcherMock`, `SafeFileHandleMock`, `RandomAccessMock`, ...), change and notification handling, and the access control, safe file handle and Unix file mode strategies |
| `Storage/` | The in-memory storage model: `InMemoryStorage`, `InMemoryContainer`, `InMemoryLocation`, drives and file handles that enforce `FileShare` |
| `Helpers/` | `Execute` (OS and framework branching, with real and simulated path implementations per OS), `ExceptionFactory` (exceptions matching the real ones), `PathHelper`, `EnumerationOptionsHelper`, `GlobPattern` |
| `Initializer/` | `FileSystemInitializer` and `DirectoryCleaner`, used by `Initialize()` and `SetCurrentDirectoryToEmptyTemporaryDirectory()` |
| `Statistics/` | Call statistics recorded by the mock systems |
| `TimeSystem/`, `RandomSystem/` | Mock time (timers, tasks, threads, stopwatch) and random/GUID generation |
| `Polyfills/` | Types and helpers missing on older frameworks, guarded by `#if` |

Key design points:

- `MockFileSystem` options choose a `SimulationMode` (Linux, macOS, Windows; .NET 6 and later only, `CAN_SIMULATE_OTHER_OS`). The mock consults its `Execute` instance for every OS-dependent decision, so code must not read the host OS directly.
- Every `MockFileSystem` owns a `MockTimeSystem` (`fileSystem.TimeSystem`) that supplies its timestamps.
- Interception (`IInterceptionHandler`) and notification (`INotificationHandler`) hooks let tests react to changes before or after they happen.
- See `Docs/Maintainer.md` and `Docs/FileSystemWatcherMock.md` for deeper notes.

## Tests

| Path | Content |
|---|---|
| `Tests/Helpers/Testably.Abstractions.TestHelpers` | `FileSystemTestBase`, `TimeSystemTestBase`, `RandomSystemTestBase`, the `[FileSystemTests]`/`[TimeSystemTests]`/`[RandomSystemTests]` data sources, `Test` (OS checks), `Skip`, `Settings`, `[AutoArguments]` (AutoFixture) |
| `Tests/Testably.Abstractions.Tests` | Folders mirror the interfaces: `FileSystem/<Type>/`, `TimeSystem/`, `RandomSystem/`; `TestHelpers/` holds project-specific helpers |
| `Tests/Testably.Abstractions.Testing.Tests` | Tests of the mock-only API, mirroring `Source/Testably.Abstractions.Testing` |
| `Tests/Testably.Abstractions.Parity.Tests` | `ParityTests`, one `Net*ParityTests` subclass per framework, shared exclusions in `TestHelpers/Parity.cs` |
| `Tests/Api/*` | API snapshot tests with `Expected/*.txt` per assembly and framework |

The test helpers reference the released `Testably.Abstractions` package and the `Testably.Abstractions.Testing` project.
