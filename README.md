![Testably.Abstractions](https://raw.githubusercontent.com/Testably/Testably.Abstractions/main/Docs/Images/social-preview.png)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FTestably.Abstractions%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/Testably.Abstractions/main)

At the core of this library are the `IFileSystem`, `IRandomSystem` and `ITimeSystem` interfaces, which allow abstracting away system dependencies.

Use these and their default implementations using your prefered dependency injection method, e.g.:
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
  IFileSystem fileSystem = new FileSystemMock();
  MyService sut = new MyService(fileSystem);

  sut.StoreData();

  var fileContent = fileSystem.File.ReadAllText("result.xml");
  // Validate fileContent
}
```

# Getting Started
- Install `Testably.Abstractions` as nuget package in your productive projects.
  ```ps
  dotnet add package Testably.Abstractions
  ```

- Install `Testably.Abstractions.Testing` as nuget package in your test projects.
  ```ps
  dotnet add package Testably.Abstractions.Testing
  ```

- If required, install `Testably.Abstractions.Extensions` as nuget package in your projects.
  ```ps
  dotnet add package Testably.Abstractions.Extensions
  ```

- Configure your dependeny injection framework, e.g. with `Microsoft.Extensions.DependencyInjections` in ASP.NET core:
  ```csharp
  builder.Services.AddSingleton<IFileSystem, FileSystem>();
  builder.Services.AddSingleton<IRandomSystem, RandomSystem>();
  builder.Services.AddSingleton<ITimeSystem, TimeSystem>();
  ```

**You can now use the interfaces in your services!**

# Abstractions
![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions)  

## File Abstractions
The `IFileSystem` interface abstracts away the following methods:
- `Path` methods
- `Directory` methods
- `File` methods

## Random Abstractions
The `IRandomSystem` interface abstracts away the following methods:
- `Random` methods
- `Guid` methods

## Time Abstractions
The `ITimeSystem` interface abstracts away the following methods:
- `DateTime` methods:  
  `DateTime.Now`, `DateTime.UtcNow` and `DateTime.Today`
- `Thread` methods:  
  `Thread.Sleep`
- `Task` methods:  
  `Task.Delay`

# Testing
![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Testing)  
In order to simplify testing, the `Testably.Abstractions.Testing` projects provides mocked instances for the abstraction interfaces:
- `FileSystemMock` allows replacing the `IFileSystem` with an in-memory file system.
- `RandomSystemMock` allows providing explicit values for Guid or random values.
- `TimeSystemMock` allows manipulating the system time and omits any delays in unit tests:
  - `DateTime.Now` and `DateTime.UtcNow` can be explicitly set.
  - `Thread.Sleep` will only sleep for 0ms, but advance the time according to the timeout.
  - `Task.Delay` will return a completed task straight away, but advance the time according to the delay.

# Extensions
![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Extensions)  
The extensions project provides some methods, based on the interfaces that simplify common tasks.
See [Examples project](/Docs/Examples) provides some examples of their usage.
