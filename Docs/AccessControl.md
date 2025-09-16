![Testably.Abstractions](https://raw.githubusercontent.com/Testably/Testably.Abstractions/main/Docs/Images/social-preview.png)
[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.AccessControl)](https://www.nuget.org/packages/Testably.Abstractions.AccessControl)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)

ACL (access control list) extension methods for [Testably.Abstractions](../README.md) using [System.IO.FileSystem.AccessControl](https://www.nuget.org/packages/System.IO.FileSystem.AccessControl/).  
Implements the methods from [FileSystemAclExtensions](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemaclextensions) on the `IFileSystem` interface.

## Example
With this library loaded you can copy ACL settings from one directory to another.
```csharp
    IFileSystem _fileSystem; // Set using DI
		
    DirectorySecurity accessControl = _fileSystem.Directory.GetAccessControl("your-directory");
    _fileSystem.Directory.SetAccessControl("another-directory", accessControl);
```
