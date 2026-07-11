# Testably.Abstractions.Compression

[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Compression)](https://www.nuget.org/packages/Testably.Abstractions.Compression)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)

Zip extensions for [`Testably.Abstractions`](https://www.nuget.org/packages/Testably.Abstractions) - adds the static methods from `System.IO.Compression.ZipFile` and `ZipArchive` to `IFileSystem`, so compression code can be tested against the in-memory `MockFileSystem`.

```ps
dotnet add package Testably.Abstractions.Compression
```

**Full documentation: [docs.testably.org/Abstractions/companion-libraries/compression](https://docs.testably.org/Abstractions/companion-libraries/compression)**

```csharp
IFileSystem fileSystem; // injected

fileSystem.ZipFile
    .CreateFromDirectory("source", "out.zip");

fileSystem.ZipFile
    .ExtractToDirectory("out.zip", "destination");

using IZipArchive archive = fileSystem.ZipFile
    .Open("out.zip", ZipArchiveMode.Update);
```

All overloads from the .NET base class library (BCL) are present, including the async variants on .NET 10+. `fileSystem.ZipArchive.New(stream, mode)` returns an `IZipArchive` that wraps `ZipArchive`, with `IZipArchiveEntry` mirroring its BCL counterpart. On `RealFileSystem` every call forwards to the underlying BCL implementation; only `MockFileSystem` routes through the in-memory zip implementation.
