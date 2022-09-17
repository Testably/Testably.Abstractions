![Testably.Abstractions](https://raw.githubusercontent.com/Testably/Testably.Abstractions/main/Docs/Images/social-preview.png)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions)

At the core of this library are the `IFileSystem` and `ITimeSystem` interfaces, which allow abstracting away system dependencies.

# File Abstractions
The `IFileSystem` interface abstracts away the following methods:
- `Path` methods

## Testing
In order to simplify testing, the `FileSystemMock` allows replacing the `IFileSystem` with a mocked instance.

---

# Time Abstractions
The `ITimeSystem` interface abstracts away the following methods:
- `DateTime` methods:  
  `DateTime.Now`, `DateTime.UtcNow` and `DateTime.Today`
- `Thread` methods:  
  `Thread.Sleep`
- `Task` methods:  
  `Task.Delay`

## Testing
In order to simplify testing, the `TimeSystemMock` allows replacing the `ITimeSystem` with a mocked instance.
- `DateTime.Now` and `DateTime.UtcNow` can be explicitly set.
- `Thread.Sleep` will only sleep for 0ms, but advance the time according to the timeout.
- `Task.Delay` will return a completed task straight away, but advance the time according to the delay.
