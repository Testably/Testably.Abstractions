# UnixFileMode example
This example highlights how to simulate working with the [`UnixFileMode`](https://learn.microsoft.com/en-us/dotnet/api/system.io.unixfilemode) in the `MockFileSystem`.

The `DefaultUnixFileModeStrategy` simulates the user and group and stores this information alongside the file/directory in the `OnSetUnixFileMode` method.
Later in the `IsAccessGranted` method you can access the stored data from the `IFileSystemExtensibility` and act accordingly to read or write requests in the `MockFileSystem`.
Grant read access when
- When the unix file mode is set to `OtherRead` OR
- When the unix file mode is set to `GroupRead` and the simulated group of the file is the same as the currently simulated group
- When the unix file mode is set to `UserRead` and the simulated user of the file is the same as the currently simulated user

The same logic applies to write access.

Simulating a user or group is done by calling `SimulateUser`/`SimulateGroup` on the `DefaultUnixFileModeStrategy`.
