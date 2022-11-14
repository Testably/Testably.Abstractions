![Testably.Abstractions](https://raw.githubusercontent.com/Testably/Testably.Abstractions/main/Docs/Images/social-preview.png)
[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Interface)](https://www.nuget.org/packages/Testably.Abstractions.Interface)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FTestably.Abstractions%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/Testably.Abstractions/main)

This library contains the abstraction interfaces for [Testably.Abstractions](../README.md), which allow replacing system dependencies:

- The `IFileSystem` interface abstracts away all I/O-related functionality from the `System.IO` namespace:  
  Static methods are directly implemented on the `IFileSystem` interface.
  Constructors are implemented as factory methods, e.g. `IFileSystem.FileInfo.New(string)` instead of `new FileInfo(string)`.
- The `ITimeSystem` interface abstracts away time-related functionality:  
  `DateTime` methods give access to the current time, `Thread` allows replacing `Thread.Sleep` and `Task` allows replacing `Task.Delay`.
- The `IRandomSystem` interface abstracts away functionality related to randomness:  
  `Random` methods implement a thread-safe Shared instance also under .NET Framework and `Guid` methods allow creating new GUIDs.
