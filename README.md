![Testably.Abstractions](https://raw.githubusercontent.com/Testably/Testably.Abstractions/main/Docs/Images/social-preview.png)  
[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Testing)](https://www.nuget.org/packages/Testably.Abstractions.Testing) 
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml) 
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions) 
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions) 

This library is a feature complete testing helper for the [IFileSystem abstractions for I/O-related functionality](https://github.com/TestableIO/System.IO.Abstractions) from the `System.IO` namespace. It uses an in-memory file system that behaves exactly like the real file system and can be used in unit tests for dependency injection.  
The testing helper also supports advanced scenarios like
- [Multiple drives with limited size](Examples/DriveManagement/README.md)
- [`FileSystemWatcher`](Examples/FileSystemWatcher/README.md) and
- a way to work with [SafeFileHandles](Examples/SafeFileHandle/README.md)

The companion projects [Testably.Abstractions.Compression](https://www.nuget.org/packages/Testably.Abstractions.Compression) and [Testably.Abstractions.AccessControl](https://www.nuget.org/packages/Testably.Abstractions.AccessControl) allow working with [Zip-Files](Examples/ZipFile/README.md) and [Access Control Lists](Examples/AccessControlLists/README.md) respectively.

As the test suite runs both against the mocked and the real file system, the behaviour between the two is identical and it also allows [simulating the file system on other operating systems](#simulating-other-operating-systems) (Linux, MacOS and Windows).

In addition, the following interfaces are defined:
- The `ITimeSystem` interface abstracts away time-related functionality:  
  - `DateTime` methods give access to the current time
  - `Task` allows replacing [`Task.Delay`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.delay)
  - `Thread` allows replacing [`Thread.Sleep`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.sleep)
  - `Timer` is a wrapper around [`System.Threading.Timer`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.timer)
- The `IRandomSystem` interface abstracts away functionality related to randomness:  
  `Random` methods implement a thread-safe Shared instance also under .NET Framework and `Guid` methods allow creating new GUIDs.

## Example
Use the interfaces and their default implementations using your prefered dependency injection method, e.g.:
```csharp
private readonly IFileSystem _fileSystem;

public class MyService(IFileSystem fileSystem)
{
    _fileSystem = fileSystem;
}

public void StoreData()
{
    var fileContent = GetFileContent();
    _fileSystem.File.WriteAllText("result.xml", fileContent);
}

private string GetFileContent()
{
    // Generate the file content
}
```

Then you test your class with the mocked types in `Testably.Abstractions.Testing`:
```csharp
[Fact]
public void StoreData_ShouldWriteValidFile()
{
    IFileSystem fileSystem = new MockFileSystem();
    MyService sut = new MyService(fileSystem);

    sut.StoreData();

    var fileContent = fileSystem.File.ReadAllText("result.xml");
    // Validate fileContent
}
```

**More examples can be found in the [examples section](Examples/README.md)!**

## Getting Started

- Install `Testably.Abstractions` as nuget package in your production projects and `Testably.Abstractions.Testing` as nuget package in your test projects.
  ```ps
  dotnet add package Testably.Abstractions
  dotnet add package Testably.Abstractions.Testing
  ```

- Configure your dependeny injection framework, e.g. with `Microsoft.Extensions.DependencyInjections` in ASP.NET core:
  ```csharp
  builder.Services
      .AddSingleton<IFileSystem, RealFileSystem>()
      .AddSingleton<IRandomSystem, RealRandomSystem>()
      .AddSingleton<ITimeSystem, RealTimeSystem>();
  ```

**You can now use the interfaces in your services!**

## Testing
In order to simplify testing, the `Testably.Abstractions.Testing` project provides mocked instances for the abstraction interfaces, which are configured using fluent syntax:

### Initialization

The following two code snippets initialize the mocked `fileSystem` with a structure like the following:
- Directory "foo"
  - Directory "bar"
  - Empty file "bar.txt"
- File "foo.txt" with "some file content" as content

```csharp
var fileSystem = new MockFileSystem();
fileSystem.Initialize().With(
    new DirectoryDescription("foo",
        new DirectoryDescription("bar"),
        new FileDescription("bar.txt")),
    new FileDescription("foo.txt", "some file content"));
```

```csharp
var fileSystem = new MockFileSystem();
fileSystem.Initialize()
	.WithSubdirectory("foo").Initialized(d => d
		.WithSubdirectory("bar")
		.WithFile("bar.txt"))
	.WithFile("foo.txt").Which(f => f.HasStringContent("some file content"));
```

### Simulating other operating systems

The `MockFileSystem` can also simulate other operating systems than the one it is currently running on. This can be achieved, by providing the corresponding `SimulationMode` in the constructor:

```csharp
var linuxFileSystem = new MockFileSystem(o => o.SimulatingOperatingSystem(SimulationMode.Linux));
// The `linuxFileSystem` now behaves like a Linux file system even under Windows:
// - case-sensitive
// - slash as directory separator

var windowsFileSystem = new MockFileSystem(o => o.SimulatingOperatingSystem(SimulationMode.Windows));
// The `windowsFileSystem` now behaves like a Windows file system even under Linux or MacOS:
// - multiple drives
// - case-insensitive
// - backslash as directory separator
```

By running all tests against the real file system and the simulated under Linux, MacOS and Windows, the behaviour is consistent between the native and simulated mock file systems.

### Drive management
```csharp
var fileSystem = new MockFileSystem();
fileSystem
    .WithDrive("D:", d => d
        .SetTotalSize(1024 * 1024))
    .InitializeIn("D:")
    .WithFile("foo.txt")
    .WithSubdirectory("sub-dir").Initialized(s => s
        .WithAFile(".json").Which(
            f => f.HasStringContent("{\"count\":1}")));
```
Initializes the mocked file system with a second drive `D:` with 1MB total available space and creates on it an empty text file `foo.txt` and a directory `sub-dir` which contains randomly named json file with `{"count":1}` as file content.

On non-Windows systems, the main drive can still be configured, e.g.
```csharp
var fileSystem = new MockFileSystem();
fileSystem.WithDrive(d => d.SetTotalSize(20));

// this will throw an IOException that there is not enough space on the disk.
fileSystem.File.WriteAllText("foo", "some text longer than 20 bytes");
```
