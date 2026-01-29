![Testably.Abstractions](https://raw.githubusercontent.com/Testably/Testably.Abstractions/main/Docs/Images/social-preview.png)
[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Compression)](https://www.nuget.org/packages/Testably.Abstractions.Compression)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)

Compression extensions for [Testably.Abstractions](https://github.com/Testably/Testably.Abstractions) using [System.IO.Compression](https://www.nuget.org/packages/System.IO.Compression/).  
Wraps the static methods from [ZipFile](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.zipfile) in an extension on `IFileSystem`.

## Example
Use the `ZipFile()` extension method to get access to `CreateFromDirectory`, `ExtractToDirectory` and `Open` methods:
```csharp
    IFileSystem _fileSystem; // Set using DI
		
    _fileSystem.ZipFile()
        .CreateFromDirectory("your-directory", "your-file.zip");

    _fileSystem.ZipFile()
        .ExtractToDirectory("your-file.zip", "your-destination");
```
