# Configuration Example
This example illustrates how the testing libraries can be configured in unit tests.

## Dependency Injection
This example shows how to use [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection) to register the abstractions in the `ServiceProvider`:
```csharp
    ServiceProvider services = new ServiceCollection()
        .AddSingleton<IFileSystem, FileSystem>()
        .AddSingleton<IRandomSystem, RandomSystem>()
        .AddSingleton<ITimeSystem, TimeSystem>()
        .BuildServiceProvider();
```

## File System

### Initialization
Shows how to initialize the file system:
```csharp
    fileSystem.InitializeIn("current-directory")
        .WithASubdirectory()
        .WithSubdirectory("foo").Initialized(s => s
            .WithAFile())
        .WithFile("bar.txt");
```
Initialize the file system in "current-directory" with
 - a randomly named directory
 - a directory named "foo" which contains a randomly named file
 - a file named "bar.txt"

 In order to use multiple drives on Windows (or network shares) you have to first register them:
 ```csharp
     fileSystem.WithDrive(@"D:", drive => drive.SetTotalSize(1024));
 ```
 The optional configuration allows limiting the maximum available space on the drive.

 ### Events
 All changes in the file system trigger certain events. All events can be
 - *intercepted*, before they occur (and e.g. an exception thrown to prevent the event from completing) on the `FileSystemMock.Intercept` property:
   ```csharp
       FileSystemMock fileSystem = new();
           fileSystem.Intercept.Creating(FileSystemTypes.File,
               _ => throw new Exception("my custom exception"));
   ```
 - *notified*, after they occured to allow a test to react to changes on the `FileSystemMock.Notify` property:
   These methods return an awaitable object that
   - Removes the notification on dispose
   - Provides a blocking mechanism until the notification happens
   ```csharp
       FileSystemMock fileSystem = new();
       fileSystem.Notify
           .OnCreated(FileSystemTypes.File, _ =>
           {
               // Do something
           })
           .ExecuteWhileWaiting(() =>
           {
               // This will trigger the callback
               fileSystem.File.Create("some-file.txt");
           })
           .Wait();
   ```

