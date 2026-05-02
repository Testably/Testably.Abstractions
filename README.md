# Testably.Abstractions

[![Testably.Abstractions](https://img.shields.io/nuget/v/Testably.Abstractions?label=Testably.Abstractions&logo=nuget)](https://www.nuget.org/packages/Testably.Abstractions)
[![Testably.Abstractions.Testing](https://img.shields.io/nuget/v/Testably.Abstractions.Testing?label=Testing&logo=nuget)](https://www.nuget.org/packages/Testably.Abstractions.Testing)
[![Build](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/Testably.Abstractions/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_Testably.Abstractions&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Testably_Testably.Abstractions)

Injectable abstractions for the static parts of the .NET BCL - file system, time and randomness - with feature-complete in-memory mocks for tests.

**📖 Full documentation: [docs.testably.org](https://docs.testably.org)**

## Quick example

```csharp
public class ReportService(IFileSystem fileSystem)
{
    public void Save(string content)
    {
        fileSystem.Directory.CreateDirectory("reports");
        fileSystem.File.WriteAllText("reports/latest.xml", content);
    }
}
```

```csharp
[Fact]
public async Task Save_WritesReportToReportsFolder()
{
    var fileSystem = new MockFileSystem();
    var sut = new ReportService(fileSystem);

    sut.Save("<report />");

    await Expect.That(fileSystem.File.ReadAllText("reports/latest.xml"))
        .IsEqualTo("<report />");
}
```

## Install

```ps
dotnet add package Testably.Abstractions
dotnet add package Testably.Abstractions.Testing
```

Then register the implementations in your DI container - see [Getting Started](https://docs.testably.org/docs/getting-started).

## Packages

| Package                              | Purpose                                                                 |
|--------------------------------------|-------------------------------------------------------------------------|
| `Testably.Abstractions`              | Production interfaces (`IFileSystem`, `ITimeSystem`, `IRandomSystem`)   |
| `Testably.Abstractions.Testing`      | `MockFileSystem`, `MockTimeSystem`, `MockRandomSystem`                  |
| `Testably.Abstractions.Compression`  | Zip / `ZipArchive` extension methods on `IFileSystem`                   |
| `Testably.Abstractions.AccessControl`| `GetAccessControl` / `SetAccessControl` on files and directories        |

## Already on TestableIO?

`Testably.Abstractions` shares the `IFileSystem` interface with [TestableIO.System.IO.Abstractions](https://github.com/TestableIO/System.IO.Abstractions), so production code stays untouched. See the [migration guide](https://docs.testably.org/docs/migration-from-testableio).

## Contributing

Issues and pull requests are welcome - see [CONTRIBUTING.md](CONTRIBUTING.md) and the [issue tracker](https://github.com/Testably/Testably.Abstractions/issues).
