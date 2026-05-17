# Testably.Abstractions.AccessControl

[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.AccessControl)](https://www.nuget.org/packages/Testably.Abstractions.AccessControl)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)

ACL extensions for [`Testably.Abstractions`](https://www.nuget.org/packages/Testably.Abstractions) - adds the methods from [`System.IO.FileSystemAclExtensions`](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemaclextensions) to `IFileSystem`, so production code that reads or writes ACLs works against both the real and the mocked file system.

```ps
dotnet add package Testably.Abstractions.AccessControl
```

**Full documentation: [docs.testably.org/Abstractions/companion-libraries/access-control](https://docs.testably.org/Abstractions/companion-libraries/access-control)**

```csharp
IFileSystem fileSystem; // injected

DirectorySecurity acl = fileSystem.Directory.GetAccessControl("data");
fileSystem.Directory.SetAccessControl("backup", acl);

fileSystem.File.WriteAllText("secret.txt", "x");
FileSecurity fileAcl = fileSystem.File.GetAccessControl("secret.txt");
```

The package adds `GetAccessControl` / `SetAccessControl` (plus the `DirectorySecurity` overloads of `CreateDirectory` and `DirectoryInfo.Create`) to `IDirectory`, `IDirectoryInfo`, `IFile`, `IFileInfo` and `FileSystemStream`. On `MockFileSystem` the security object is stored with the entry and returned unchanged on read.

> ⚠️ The ACL APIs are marked `[SupportedOSPlatform("windows")]`. Calling them on Linux or macOS throws `PlatformNotSupportedException`.
