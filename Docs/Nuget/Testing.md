# Testably.Abstractions.Testing

[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Testing)](https://www.nuget.org/packages/Testably.Abstractions.Testing)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)

Testing helpers for [`Testably.Abstractions`](https://www.nuget.org/packages/Testably.Abstractions) - in-memory `MockFileSystem`, `MockTimeSystem` and `MockRandomSystem` that behave identically to the .NET base class library (BCL) but stay deterministic and never touch disk, the system clock or randomness.

```ps
dotnet add package Testably.Abstractions.Testing
```

**Full documentation: [docs.testably.org/Abstractions](https://docs.testably.org/Abstractions/)**

The test suite runs every assertion against both the real and the mocked file system, so the mock behaves identically to the BCL. Highlights:

- **MockFileSystem** - fluent `Initialize()` API, multiple drives with size limits, `FileSystemWatcher`, `SafeFileHandle`, file-version metadata and unix file modes.
- **Cross-platform simulation** - run a Linux, macOS or Windows file system regardless of the host via `new MockFileSystem(o => o.SimulatingOperatingSystem(SimulationMode.Linux))`.
- **Intercept and Notify** - inject exceptions before a file-system or time-system operation completes, or react to them after the fact (including replay of events that fired before the subscription).
- **MockTimeSystem** - control `DateTime.Now`, advance time manually or via auto-advance, mock `Timer`/`PeriodicTimer` with persistent execution counters.
- **MockRandomSystem** - seed `Random` and `Guid.NewGuid()` for reproducible tests.
- **Statistics** - inspect how the system-under-test used each abstraction (`fileSystem.Statistics`).
