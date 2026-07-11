# Testably.Abstractions.MemoryMappedFiles

[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.MemoryMappedFiles)](https://www.nuget.org/packages/Testably.Abstractions.MemoryMappedFiles)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)

Memory-mapped file extensions for [`Testably.Abstractions`](https://www.nuget.org/packages/Testably.Abstractions) - adds the methods from `System.IO.MemoryMappedFiles.MemoryMappedFile` to `IFileSystem`, so memory-mapped-file code can be tested against the in-memory `MockFileSystem`.

```ps
dotnet add package Testably.Abstractions.MemoryMappedFiles
```

```csharp
IFileSystem fileSystem; // injected

using IMemoryMappedFile mappedFile = fileSystem.MemoryMappedFile
    .CreateFromFile("data.bin");

using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
accessor.Write(0, 42);
int value = accessor.ReadInt32(0);
```

The abstraction is exposed as the extension property `fileSystem.MemoryMappedFile` (note: no `()`), returning an `IMemoryMappedFileFactory`. On `RealFileSystem` every call forwards to the underlying base class library (BCL) implementation; on `MockFileSystem` the views are built directly over the in-memory file bytes.

Some parts of the BCL surface are intentionally not abstracted, because they have no meaningful in-memory equivalent:

- `CreateNew`, `CreateOrOpen` and `OpenExisting` operate on operating-system shared memory (named or anonymous) rather than a file. They forward normally on `RealFileSystem`, but throw `NotSupportedException` on `MockFileSystem`.
- The `SafeMemoryMappedFileHandle` / `SafeMemoryMappedViewHandle` handle and pointer APIs, as well as the `SafeFileHandle`-based `CreateFromFile` overload, are not exposed at all (mirroring how the `SafeFileHandle`-based `FileStream` construction is excluded elsewhere).

On `MockFileSystem` the reads and writes, their capacity handling and the thrown exceptions mirror the real `MemoryMappedViewAccessor` / `MemoryMappedViewStream`. Two details tied to operating-system memory mapping are intentionally simplified: a view created without an explicit size has a `Capacity` of exactly the remaining bytes (the real file system rounds up to the system page size), and `PointerOffset` is always `0`.
