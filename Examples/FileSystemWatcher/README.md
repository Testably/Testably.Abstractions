# Working with the `FileSystemWatcher`
This example illustrates how to use the mocked [`FileSystemWatcher`](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher).

In the example we listen to a `Deleted` event and set a [`ManualResetEventSlim`](https://learn.microsoft.com/de-de/dotnet/api/system.threading.manualreseteventslim).
After deleting a directory this event is triggered within 1s.
