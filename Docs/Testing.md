![Testably.Abstractions](https://raw.githubusercontent.com/Testably/Testably.Abstractions/main/Docs/Images/social-preview.png)
[![Nuget](https://img.shields.io/nuget/v/Testably.Abstractions.Testing)](https://www.nuget.org/packages/Testably.Abstractions.Testing)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&branch=main&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_Testably.Abstractions&branch=main)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FTestably.Abstractions%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/Testably.Abstractions/main)

This library contains the testing helpers for [Testably.Abstractions](../README.md).

## MockFileSystem

### Initialization
Shows how to initialize the file system:
```csharp
    fileSystem.InitializeIn("current-directory")
        .WithASubdirectory()
        .WithSubdirectory("foo").Initialized(s => s
            .WithAFile())
        .WithFile("bar.txt");
```
Initialize the file system in "current-directory" with
- a randomly named directory
- a directory named "foo" which contains a randomly named file
- a file named "bar.txt"

In order to use multiple drives on Windows (or network shares) you have to first register them:
```csharp
    fileSystem.WithDrive(@"D:", drive => drive.SetTotalSize(1024));
```
The optional configuration allows limiting the maximum available space on the drive.

### Events
All changes in the file system trigger certain events. All events can be
- _intercepted_, before they occur (and e.g. an exception thrown to prevent the event from completing) on the `Intercept` property:
  ```csharp
      MockFileSystem fileSystem = new();
          fileSystem.Intercept.Creating(FileSystemTypes.File,
              _ => throw new Exception("my custom exception"));
  ```
- _notified_, after they occured to allow a test to react to changes on the `MockFileSystem.Notify` property:
  These methods return an awaitable object that
  * Removes the notification on dispose
  * Provides a blocking mechanism until the notification happens
  ```csharp
      MockFileSystem fileSystem = new();
      fileSystem.Notify
          .OnCreated(FileSystemTypes.File, _ =>
          {
              // Do something
          })
          .ExecuteWhileWaiting(() =>
          {
              // This will trigger the callback
              fileSystem.File.Create("some-file.txt");
          })
          .Wait();
  ```