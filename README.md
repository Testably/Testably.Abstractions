![Testably.Abstractions](https://raw.githubusercontent.com/Testably/Testably.Abstractions/main/Docs/Images/social-preview.png)
[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions)](https://www.nuget.org/packages/Testably.Abstractions)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/5b9b2f79950447a69d69037b43acd590)](https://www.codacy.com/gh/Testably/Testably.Abstractions/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Testably/Testably.Abstractions&amp;utm_campaign=Badge_Grade)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FTestably.Abstractions%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/Testably.Abstractions/main)

This library is an alternative testing helper for the [IFileSystem interfaces from System.IO.Abstractions](https://github.com/TestableIO/System.IO.Abstractions).
These interfaces allow abstracting away all I/O-related functionality from the `System.IO` namespace.  
In addition, the following interfaces are defined:
- The `ITimeSystem` interface abstracts away time-related functionality:  
  `DateTime` methods give access to the current time, `Thread` allows replacing `Thread.Sleep` and `Task` allows replacing `Task.Delay`.
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

- Install `Testably.Abstractions` as nuget package in your productive projects and `Testably.Abstractions.Testing` as nuget package in your test projects.
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
In order to simplify testing, the `Testably.Abstractions.Testing` projects provides mocked instances for the abstraction interfaces:

These mocks are configured using fluent syntax:
```csharp
new MockFileSystem()
    .WithDrive("D:", d => d
        .SetTotalSize(1024*1024))
    .Initialize()
        .WithAFile()
        .WithASubdirectory().Initialized(s => s
            .WithAFile(".txt"));
```
Initializes the mocked file system with a second drive `D:` with 1MB total available space and creates on the default drive `C:` a random file and a random sub-directory containing a ".txt" file.
