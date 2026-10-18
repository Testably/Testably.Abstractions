# Development workflows

Commands are run from the repository root. Examples use PowerShell on Windows; `./build.sh` is the equivalent of `./build.ps1` on Linux and macOS.

## Build

```ps
dotnet build Testably.Abstractions.slnx
dotnet build Testably.Abstractions.slnx -p:NetCoreOnly=true
```

- Building the solution compiles every target framework of every project and takes a few minutes on a cold start.
- Source projects have `GeneratePackageOnBuild` and reference their readme through `$(SolutionDir)`. `dotnet build Source/<Project>` therefore compiles but fails in the pack step; build through the solution, or a test project that references the source project.
- Local builds use the placeholder version `255.255.255.255`; the pipeline's `Compile` target sets the real version from GitVersion.
- `-p:NetCoreOnly=true` removes `net48` from the test projects. `-p:NetFrameworkOnly=true` builds sources for `netstandard2.0` and tests for `net48` only.

## Test

Test projects are TUnit executables on Microsoft.Testing.Platform (selected in `global.json`). Pass test platform options directly to `dotnet test`:

```ps
dotnet test --project Tests/Testably.Abstractions.Tests -f net10.0
dotnet test --project Tests/Testably.Abstractions.Tests -f net10.0 --treenode-filter "/*/*/ExistsTests/*"
dotnet test --project Tests/Testably.Abstractions.Tests -f net10.0 --treenode-filter "/*/Testably.Abstractions.Tests.FileSystem.Directory/*/*"
dotnet test --project Tests/Testably.Abstractions.Testing.Tests -f net10.0 --no-build --treenode-filter "/*/*/MockTimeSystemTests/*"
```

- The tree node filter has the form `/<assembly>/<namespace>/<class>/<test>` and accepts `*` wildcards.
- Without `-f` every target framework runs; `net48` only runs on Windows.
- Debug runs skip the real file system rows by default; see [testing.md](testing.md#test-settings). `-c Release` includes them.
- Results and reports land in `TestResults/`.

| Project | Covers |
|---|---|
| `Tests/Testably.Abstractions.Tests` | Behaviour of the file, time and random abstractions, run against the real implementations and the mocks |
| `Tests/Testably.Abstractions.Testing.Tests` | Mock-only features: initialization, notifications, interception, statistics, simulation modes, storage |
| `Tests/Testably.Abstractions.Parity.Tests` | Interfaces mirror the real BCL types for each target framework |
| `Tests/Testably.Abstractions.Compression.Tests` | `Testably.Abstractions.Compression` |
| `Tests/Testably.Abstractions.AccessControl.Tests` | `Testably.Abstractions.AccessControl` (Windows ACLs) |
| `Tests/Testably.Abstractions.MemoryMappedFiles.Tests` | `Testably.Abstractions.MemoryMappedFiles` |
| `Tests/Api/Testably.Abstractions.Api.Tests` | Public API snapshots of the main packages and `Testably.Abstractions` |
| `Tests/Api/Testably.Abstractions.Core.Api.Tests` | Public API snapshots of the two interface packages |

## Public API snapshots

The API tests load the compiled assemblies from `Source/<Assembly>/bin/<Configuration>/<framework>/` for every framework listed in `Source/Directory.Build.props` and compare them with `Tests/Api/<Project>/Expected/<Assembly>_<framework>.txt`. Build the solution first; otherwise frameworks that the test project does not reference fail with `FileNotFoundException`.

```ps
dotnet build Testably.Abstractions.slnx
dotnet test --project Tests/Api/Testably.Abstractions.Api.Tests --no-build
dotnet test --project Tests/Api/Testably.Abstractions.Core.Api.Tests --no-build
```

When a change to the public API is intended, run the explicit acceptance test of the affected project to rewrite the expected files, then review the diff:

```ps
dotnet test --project Tests/Api/Testably.Abstractions.Api.Tests --no-build --treenode-filter "/*/*/ApiAcceptance/AcceptApiChanges"
```

## Build pipeline

`build.ps1`/`build.sh`/`build.cmd` build and run `Pipeline/Build.csproj`, a [Fallout](https://www.nuget.org/packages/Fallout.Common) build (a NUKE fork). `./build.ps1 --help` lists the targets. Without a target it runs `ApiChecks` and `UnitTests`. The configuration defaults to Debug locally and Release on the server (`--configuration`).

| Target | Purpose |
|---|---|
| `Compile` | Restore, calculate the Core and main versions with GitVersion (`core/v` tag prefix for Core) and build the projects of the current `BuildScope` |
| `ApiChecks` | Run the API test projects of the current `BuildScope` |
| `UnitTests` | Run the six unit test projects for every target framework (`net48` only on Windows) with TRX and Cobertura output; retried once on failure |
| `CodeCoverage` | `UnitTests` plus an OpenCover report via ReportGenerator |
| `CodeAnalysis` | SonarCloud begin/end around `Compile` and `CodeCoverage`; needs `SonarToken` |
| `Pack` | Collect the packages into `Artifacts/Packages/Core` and `Artifacts/Packages/Main` |
| `MutationTestsWindows`, `MutationTestsLinux`, `MutationTests` | Stryker.NET mutation tests (see below) |
| `MutationComment` | Post or update the mutation summary on the pull request |

`BuildScope` (`Pipeline/BuildScope.cs`, set in `Pipeline/Build.cs`) is `Default`, `CoreOnly` or `MainOnly`. It limits `Compile`, `Pack` and `ApiChecks` to the Core or main projects and disables unit tests, coverage, Sonar and mutation tests where they do not apply. Use `CoreOnly` while a Core change waits for its release, because the main projects still build against the released `Testably.Abstractions.Interface` package.

`Compile` also writes `Artifacts/README.md`, a copy of `README.md` with version-specific badges, which the `Testably.Abstractions` package uses as its readme.

## Mutation tests

```ps
./build.ps1 MutationTestsWindows
./build.ps1 MutationTestsLinux
```

The targets install `dotnet-stryker` into `TestResults/dotnet-stryker` and mutate on `net10.0`; reports go to `Artifacts/Stryker`. `MutationTestsWindows` covers `Testably.Abstractions`, `.AccessControl`, `.Compression` and `.MemoryMappedFiles`; `MutationTestsLinux` covers `Testably.Abstractions.Testing`, with exclusions for OS-specific helpers and exception messages listed in `Pipeline/Build.MutationTests.cs`. Off `main` only changed code is mutated (`since: main`). A full run takes a long time; only run it when asked. The mutation jobs in `ci.yml` are currently disabled (`if: false`).

## Documentation

- `Docs/pages/docs/**` is the user documentation published at <https://docs.testably.org/Abstractions/>. A push to `main` that touches it triggers `notify-docs-site.yml`, which asks `Testably/Testably.Site` to rebuild. There is no local docs build in this repository.
- `README.md` is the readme of `Testably.Abstractions`; `Docs/Nuget/*.md` are the readmes of the other packages.
- `Docs/Maintainer.md` and `Docs/FileSystemWatcherMock.md` hold design notes on the mocks.
- `Docs/agents/` (this folder) is for contributors and agents and is not published.

## CI and release

| Workflow | Trigger | Jobs |
|---|---|---|
| `ci.yml` | Pull requests to `main` | Unit tests (`CodeCoverage`) on Ubuntu, Windows and macOS, `ApiChecks`, SonarCloud (same-repo PRs only), test result publishing; mutation jobs disabled |
| `build.yml` | Push to `main`, `v*` and `core/v*` tags | The same checks plus `Pack`; a `core/v*` tag pushes `Artifacts/Packages/Core`, a `v*` tag pushes `Artifacts/Packages/Main` to nuget.org via trusted publishing and creates a GitHub release for non-prerelease tags |
| `notify-docs-site.yml` | Push to `main` touching `Docs/pages/docs/**` | Dispatches a rebuild of the docs site |

Renovate keeps dependencies current; `.github/mergify.yml` squash-merges its PRs once the unit and API test checks pass. All checkouts use `fetch-depth: 0` because GitVersion needs the full history.
