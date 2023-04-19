# Examples
This is an overview of the provided examples for "Testably.Abstractions".

- **[Access Control Lists (ACL)](AccessControlLists/README.md)**  
  This example illustrates how to work with access control lists in the `MockFileSystem`.

- **[Configuration](Configuration/README.md)**  
  This example illustrates how the testing libraries can be configured in unit tests.

- **[Driver Management](DriverManagement/README.md)**  
  This example illustrates how to define multiple drives and use space management on individual drives.

- **[File-System Watcher](FileSystemWatcher/README.md)**  
  This example illustrates how to use the mocked [`FileSystemWatcher`](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher).

- **[SafeFileHandle](SafeFileHandle/README.md)**  
  This example illustrates how to work with `SafeFileHandle` in the `MockFileSystem`.

- **[Thread-aware time provider](ThreadAwareTimeProvider/README.md)**  
  The default implementation of the `ITimeProvider` uses a `DateTime` property to simulate the current time, that is advanced with every `Thread.Sleep` or `Task.Delay` call.
  In a scenario with multiple threads running in parallel, these would each influence each other differently in the mocked instance than "in the real world".  
  This example illustrates how to implement a thread-aware time provider for such a scenario.

- **[Zip-File](ZipFile/README.md)**  
  This example highlights how to use the `IFileSystem` to compress and uncompress directories using `System.IO.Compression`.
