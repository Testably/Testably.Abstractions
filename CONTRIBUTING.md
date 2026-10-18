# Contributor guide

## Pull requests
**Pull requests are welcome!**  
Please include a clear description of the changes you have made with your request. The title should follow the [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) guideline with one of the following [prefixes](https://github.com/conventional-changelog/commitlint/tree/master/%40commitlint/config-conventional):
- `feat:` A new feature
- `fix:` A bug fix
- `docs:` Documentation only changes
- `style:` Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- `refactor:` A code change that neither fixes a bug nor adds a feature
- `perf:` A code change that improves performance
- `test:` Adding missing tests or correcting existing tests
- `build:` Changes that affect the build system or external dependencies
- `ci:` Changes to our CI configuration files and scripts
- `chore:` Other changes that don't modify src or test files
- `revert:` Reverts a previous commit

All code should be covered by unit tests and comply with the coding guideline in this project.

**Technical expectation**  
As a framework for supporting unit testing, this project has a high standard for testing itself.  
In order to support this, static code analysis is performed using [SonarCloud](https://sonarcloud.io/project/overview?id=Testably_Testably.Abstractions) with quality gate requiring to  
- solve all issues reported by SonarCloud
- have a code coverage of > 90%

Additionally the quality of the unit tests can be checked with [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/) mutation tests (`./build.ps1 MutationTestsWindows` and `./build.sh MutationTestsLinux`). The mutation jobs in the CI workflow are currently disabled.

## Tests
On the build system, unit tests are executed both against the `MockFileSystem` and the `RealFileSystem`. This ensures that the tests verify correct assumptions.
In order to simplify and speedup the development process, per default, some tests are disabled in DEBUG mode.
These can be enabled by changing the defaults in [`Settings.cs`](https://github.com/Testably/Testably.Abstractions/blob/main/Tests/Helpers/Testably.Abstractions.TestHelpers/Settings.cs) locally:
- `LongRunningTests` (`AlwaysEnabled` / `DisabledInDebugMode` (default) / `AlwaysDisabled`)
  Some tests take a long time to run against the real file system (e.g. timeout). Per default, they are disabled in DEBUG mode.
- `RealFileSystemTests` (`AlwaysEnabled` / `DisabledInDebugMode` (default) / `AlwaysDisabled`)
  All tests against the real file system. Per default, they are disabled in DEBUG mode.
- `BrittleTests` (`AlwaysEnabled` / `DisabledInDebugMode` / `AlwaysDisabled` (default))
  Timing-sensitive tests against the real time system. Per default, they are always disabled.

*Note: Do not commit changes to these defaults, so that they only affect the individual developer!*

The test projects are [TUnit](https://tunit.dev/) executables on Microsoft.Testing.Platform. Run them with `dotnet test --project <path>`, e.g. `dotnet test --project Tests/Testably.Abstractions.Tests -f net10.0 --treenode-filter "/*/*/ExistsTests/*"`.

## Documentation
To get a better understanding of the different mock implementations, see [Maintainer.md](./Docs/Maintainer.md).
The document outlines the different behaviors the mocks replicate.
