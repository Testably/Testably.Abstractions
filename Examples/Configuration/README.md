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

 ### Events
 All changes in the file system trigger certain events. All events can be
 - intercepted, before they occur (and e.g. an exception thrown to prevent the event from completing)
 - notified, after they occured to allow a test to react to changes
