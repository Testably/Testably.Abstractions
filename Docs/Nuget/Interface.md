# Testably.Abstractions.Interface

[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Interface)](https://www.nuget.org/packages/Testably.Abstractions.Interface)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)

Interfaces for [`Testably.Abstractions`](https://www.nuget.org/packages/Testably.Abstractions) - `IFileSystem`, `ITimeSystem` and `IRandomSystem` for abstracting the static parts of the .NET base class library (BCL).

```ps
dotnet add package Testably.Abstractions.Interface
```

**Full documentation: [docs.testably.org/Abstractions](https://docs.testably.org/Abstractions/)**

> Most users install [`Testably.Abstractions`](https://www.nuget.org/packages/Testably.Abstractions) (production) and [`Testably.Abstractions.Testing`](https://www.nuget.org/packages/Testably.Abstractions.Testing) (tests) instead - both pull these interfaces in transitively.

- `IFileSystem` mirrors `System.IO` (`File`, `Directory`, `FileInfo`, `DirectoryInfo`, `FileStream`, `Path`, `DriveInfo`, `FileSystemWatcher`, `FileVersionInfo`). Constructors are exposed as factory methods (e.g. `fileSystem.FileInfo.New(path)`). Lives in the `System.IO.Abstractions` namespace, so it is source-compatible with [`TestableIO.System.IO.Abstractions`](https://github.com/TestableIO/System.IO.Abstractions).
- `ITimeSystem` covers `DateTime`, `Stopwatch`, `Task.Delay`, `Thread.Sleep`, `Timer` and (on supported targets) `PeriodicTimer`.
- `IRandomSystem` covers `Random` (with a thread-safe `Shared` instance on all targets) and `Guid`.
