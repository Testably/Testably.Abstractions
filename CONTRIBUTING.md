# Contributor guide

## Pull requests
**Pull requests are welcome!**  
Please include a clear description of the changes you have made with your request; the title should follow the [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) guideline.
All code should be covered by unit tests and comply with the coding guideline in this project.

### Technical expectations
As a framework for supporting unit testing, this project has a high standard for testing itself.  
In order to support this, static code analysis is performed using [SonarCloud](https://sonarcloud.io/project/overview?id=Testably_Testably.Abstractions) with quality gate requiring to  
- solve all issues reported by SonarCloud
- have a code coverage of > 90%

Additionally each push to the `main` branch checks the quality of the unit tests using [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/).

## Tests
On the build system, unit tests are executed both against the `MockFileSystem` and the `RealFileSystem`. This ensures that the tests verify correct assumptions.
In order to simplify and speedup the development process, per default, some tests are disabled in DEBUG mode.
These can be enabled by explicitely running the [`Testably.Abstractions.TestSettings`](https://github.com/Testably/Testably.Abstractions/tree/main/Tests/Settings/Testably.Abstractions.TestSettings) tests:
- `LongRunningTests` (`AlwaysEnabled` / `DisabledInDebugMode` (default) / `AlwaysDisabled`)
  Some tests take a long time to run against the real file system (e.g. timeout). Per default, they are disabled in DEBUG mode.
- `RealFileSystemTests` (`AlwaysEnabled` / `DisabledInDebugMode` (default) / `AlwaysDisabled`)
  All tests against the real file system. Per default, they are disabled in DEBUG mode.

*Note: These settings are stored locally in `test.settings.json` which is excluded in [`.gitignore`](https://github.com/Testably/Testably.Abstractions/blob/main/.gitignore) so that it only affects the individual developer!*

## Versioning
This project uses [MinVer](https://github.com/adamralph/minver) for versioning.  
Tags are automatically added during a release build. In order to trigger a release, create a release branch. After all tests are successful and the manual check is approved, that Tag is automatically applied and Nuget Packages are pushed to [nuget.org](https://www.nuget.org/packages/Testably.Abstractions).
Release branches must follow the following naming convention:
```markdown
    `release/v{major}.{minor}.{revision}`
```
