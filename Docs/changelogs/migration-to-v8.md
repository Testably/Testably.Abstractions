---
description: How to upgrade Testably.Abstractions.Testing from v7 to v8.
---

# Migration to v8

Version 8 adds support for .NET 11: the packages target `net11.0`, and the mocks and the compression package implement the new .NET 11 APIs.

Version 8 of `Testably.Abstractions.Testing` also replaces the extension point interfaces of the mocks with classes, so that they can grow without breaking every implementer, and changes how `MockTimeSystem` fires timers. Tests that only use the mocks without custom strategies and without timers need no changes.

## .NET 11 support

### New APIs

The interfaces mirror the new .NET 11 APIs, and the mocks implement them:

- **Random system:** `IRandom.NextInteger<T>()`, `NextInteger<T>(T)`, `NextInteger<T>(T, T)` and `NextBinaryFloat<T>()`. `RandomMock` uses its configured generators for `int`, `long`, `float` and `double`.
- **Hard links:** `IFile.CreateHardLink` and `IFileInfo.CreateAsHardLink`. In `MockFileSystem`, all links of a file share its content, attributes, timestamps and Unix file mode. `FileShare` locks apply to the file, so opening one link without sharing blocks the others, and the content and drive space are only released when the last link is deleted.
- **Null handles:** `IFile.OpenNullHandle` returns a handle that discards written bytes and returns no data. Like the real handle, it only supports seeking on Linux and macOS.
- **Compression:** `IZipArchive.CreateEntry`, `CreateEntryFromFile` and `CreateEntryFromFileAsync` with a password and a `ZipEncryptionMethod`; `ExtractToDirectory(Async)` and `ExtractToFile(Async)` with `ZipExtractionOptions`; `IZipFile.CreateFromDirectory(Async)` with `ZipFileCreationOptions`; `IZipArchiveEntry.Open(Async)` with a `FileAccess` and/or a password, and the properties `CompressionMethod`, `EncryptionMethod` and `VersionMadeBy`.

```csharp
MockFileSystem fileSystem = new();
fileSystem.File.WriteAllText("original.txt", "foo");
fileSystem.File.CreateHardLink("link.txt", "original.txt");

fileSystem.File.WriteAllText("original.txt", "bar");

await That(fileSystem.File.ReadAllText("link.txt")).IsEqualTo("bar");
```

### Behavior changes on .NET 11

On `net11.0`, the mocks follow these behavior changes of the .NET 11 runtime:

- `IFileInfo.Open`, `OpenRead`, `OpenWrite` and `OpenText` refresh the cached state of the `IFileInfo`, like `Create` already did ([dotnet/runtime#124429](https://github.com/dotnet/runtime/pull/124429)).
- On Linux and macOS, file streams are never asynchronous, because Unix opens regular files without `O_NONBLOCK` ([breaking change](https://learn.microsoft.com/dotnet/core/compatibility/core-libraries/11/safefilehandle-isasync-unix), [dotnet/runtime#125220](https://github.com/dotnet/runtime/pull/125220)).
- A stream created from a handle follows the handle and ignores the `isAsync` argument ([breaking change](https://learn.microsoft.com/dotnet/core/compatibility/core-libraries/11/safefilehandle-isasync-unix)).
- On Linux and macOS, `CreateHardLink` with a missing target throws a `DirectoryNotFoundException` when the link path has no directory part ([dotnet/runtime#120041](https://github.com/dotnet/runtime/pull/120041)).
- On Linux and macOS, `ITimer.Dispose(WaitHandle)` with a `Mutex` or `Semaphore` disposes the timer and then throws an `InvalidOperationException`.

## File system strategies are abstract base classes

The strategy interfaces of `MockFileSystem` are replaced with abstract base classes. A custom strategy derives from the base class and overrides its members instead of implementing an interface.

| v7 | v8 |
|----|----|
| `IAccessControlStrategy` | `AccessControlStrategy` |
| `IUnixFileModeStrategy` | `UnixFileModeStrategy` |
| `ISafeFileHandleStrategy` | `SafeFileHandleStrategy` |

`WithAccessControlStrategy`, `WithUnixFileModeStrategy` and `WithSafeFileHandleStrategy` take the base classes. The built-in strategies (`DefaultAccessControlStrategy`, `DefaultSafeFileHandleStrategy`, `NullSafeFileHandleStrategy` and `NullUnixFileModeStrategy`) work unchanged.

### Access control

`AccessControlStrategy` no longer has the `IsAccessGranted(fullPath, extensibility)` overload without the `AccessRequest`, and `DefaultAccessControlStrategy` no longer has the constructor with a two-argument callback. Add the `AccessRequest` parameter to the callback:

```csharp
// v7
new DefaultAccessControlStrategy((path, extensibility) => !path.EndsWith(".lock"));

// v8
new DefaultAccessControlStrategy((path, extensibility, _) => !path.EndsWith(".lock"));
```

### Unix file mode

`UnixFileModeStrategy.IsAccessGranted` receives an `AccessRequest` instead of the requested `FileAccess`; read it from `request.Access`. `OnSetUnixFileMode` is virtual and does nothing by default, so only override it when you need it.

```csharp
// v7
public class MyStrategy : IUnixFileModeStrategy
{
	public bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility,
		UnixFileMode mode, FileAccess requestedAccess)
		=> requestedAccess == FileAccess.Read || mode.HasFlag(UnixFileMode.UserWrite);

	public void OnSetUnixFileMode(string fullPath, IFileSystemExtensibility extensibility,
		UnixFileMode mode)
	{
	}
}

// v8
public class MyStrategy : UnixFileModeStrategy
{
	public override bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility,
		UnixFileMode mode, AccessRequest request)
		=> request.Access == FileAccess.Read || mode.HasFlag(UnixFileMode.UserWrite);
}
```

See [access control](../03-file-system/07-access-control.mdx), [Unix file mode](../03-file-system/08-unix-file-mode.mdx) and [safe file handles](../03-file-system/09-safe-file-handles.mdx).

## `ITimerStrategy` is replaced with `TimerStrategy`

`ITimerStrategy` is removed, and `TimerStrategy` is a sealed class. `MockTimeSystem.WithTimerStrategy` takes a `TimerStrategy` and `TimerStrategy.Default` returns one. Replace references to `ITimerStrategy` with `TimerStrategy`. Code that creates a strategy with `new TimerStrategy(...)` keeps working unchanged; a custom implementation of `ITimerStrategy` creates a `TimerStrategy` with the same `TimerMode` and `swallowExceptions` setting instead.

## Mocked timers fire in order

`MockTimeSystem` fires due timers in the order of their due time, with the clock at their due time, and no longer lets timeouts move the clock. Check tests that use timers or auto-advance for the following changes:

- A timer callback sees its own due time instead of the final time of an advance.
- With auto-advance, a one-shot timer whose state is a `CancellationTokenSource` is a timeout and no longer moves the clock on its own. This includes the timers of `new CancellationTokenSource(TimeSpan, TimeProvider)` and `Task.WaitAsync(TimeSpan, TimeProvider)` on `ToTimeProvider()`. Code that waits for nothing but a timeout, e.g. `Task.Delay(Timeout.InfiniteTimeSpan, timeProvider, cts.Token)`, waits until something else moves the clock. Move the clock with `AdvanceBy` or give such a timer a different state.
- With auto-advance, parallel `Thread.Sleep` and `Task.Delay` waits overlap like real ones instead of adding up: two parallel one-second sleeps move the clock by one second, not two. Explicit `AdvanceBy` calls still add up.
- `Task.Delay` returns a canceled task for a canceled token instead of throwing, like the real one. Await the task to observe the `OperationCanceledException`.

See [auto-advance](../04-time-system/02-auto-advance.mdx) and [timers](../04-time-system/04-timers.mdx).
