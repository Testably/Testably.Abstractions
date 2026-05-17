# Testably.Abstractions.FileSystem.Interface

[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.FileSystem.Interface)](https://www.nuget.org/packages/Testably.Abstractions.FileSystem.Interface)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)

File-system interface for [`Testably.Abstractions`](https://www.nuget.org/packages/Testably.Abstractions) - defines `IFileSystem` (and its sub-types) in the `System.IO.Abstractions` namespace, making it source-compatible with [`TestableIO.System.IO.Abstractions`](https://github.com/TestableIO/System.IO.Abstractions).

```ps
dotnet add package Testably.Abstractions.FileSystem.Interface
```

**Full documentation: [docs.testably.org/Abstractions](https://docs.testably.org/Abstractions/)**

> Most users install [`Testably.Abstractions`](https://www.nuget.org/packages/Testably.Abstractions) (production) and [`Testably.Abstractions.Testing`](https://www.nuget.org/packages/Testably.Abstractions.Testing) (tests) instead - both pull this package in transitively. Reach for this package directly only if you need just the file-system interface, without `ITimeSystem` / `IRandomSystem`.

`IFileSystem` mirrors `System.IO` (`File`, `Directory`, `FileInfo`, `DirectoryInfo`, `FileStream`, `Path`, `DriveInfo`, `FileSystemWatcher`, `FileVersionInfo`). Constructors are exposed as factory methods (e.g. `fileSystem.FileInfo.New(path)`).
