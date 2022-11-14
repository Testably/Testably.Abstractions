# Working with `SafeFileHandle`s
Example how to work with `SafeFileHandle` in the `MockFileSystem`.

In order to be able to support `SafeFileHandle`, the `MockFileSystem` provides a `ISafeFileHandleStrategy`
that can be set via `MockFileSystem.WithSafeFileHandleStrategy(...)` and which maps a `SafeFileHandle fileHandle`
to a `SafeFileHandleMock`.  
This object points to a location in the `MockFileSystem` instead.

The included default implementation uses a callback to map `SafeFileHandle` to a `SafeFileHandleMock`.

## Example
The example illustrates how to create a custom implementation of the `ISafeFileHandleStrategy` which stores the
mapping internally and synchronizes the attributes, creation time, last access time and last write time on access
from the real file system to the mock file system.

A test case verifies, that the last access time from the real file system is returned when using the safe file handle
on the mock file system:
```csharp
    ISafeFileHandleStrategy strategy = new CustomSafeFileHandleStrategy(...);
    MockFileSystem mockFileSystem = new MockFileSystem()
	    .WithSafeFileHandleStrategy(strategy)
    SafeFileHandle fileHandle = UnmanagedFileLoader.CreateSafeFileHandle(pathToFileOnRealFileSystem);
    DateTime lastAccessTimeOnMockFileSystem = mockFileSystem.File.GetLastAccessTime(fileHandle);
```
