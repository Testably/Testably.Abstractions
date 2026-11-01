# Testably.Abstractions Agent Guide

Testably.Abstractions provides injectable abstractions for the static parts of the .NET base class library (file system, time, randomness) and in-memory mocks that behave like the real implementations. Read this file first; open the docs under [References](#references) only when a task needs them.

## Setup and build

- The SDK comes from `global.json` (.NET 11, `rollForward: latestMinor`), which also selects `Microsoft.Testing.Platform` as the `dotnet test` runner. CI installs the 6.0, 8.0, 9.0 and 10.0 runtimes and the SDK from `global.json`; local test runs need the runtime of each framework you target.
- Build through the solution: `dotnet build Testably.Abstractions.slnx`. Source projects pack on build, so `dotnet build` on a single source project also produces its package.
- Source projects target `net6.0;net8.0;net9.0;net10.0;net11.0;netstandard2.1;netstandard2.0` (`Source/Directory.Build.props`), tests target `net8.0;net9.0;net10.0;net11.0;net48` (`Tests/Directory.Build.props`). `-p:NetCoreOnly=true` drops `net48` from the tests; `-p:NetFrameworkOnly=true` reduces sources to `netstandard2.0` and tests to `net48`.
- API differences between frameworks are expressed with `FEATURE_*` constants defined in `Feature.Flags.props` (plus `NETFRAMEWORK` and `CAN_SIMULATE_OTHER_OS`). For a newly mirrored API, prefer adding a flag there over raw `NETx_0_OR_GREATER` checks.
- Package versions are central in `Directory.Packages.props`. Sources build with `TreatWarningsAsErrors`, nullable enabled and Meziantou.Analyzer.
- The build pipeline is `./build.ps1 <Target>` / `./build.sh <Target>` (a Fallout project in `Pipeline/`). GitVersion needs the full git history, so do not work in a shallow clone.

## Design goals

Behaving like the real system is a must. The other goals are strong defaults and win in this order when they conflict: stable and compatible, a minimal public API, a consistent and pleasant mock-only API, extensible, thread-safe mock state, a fast happy path, documented. [Docs/agents/design-goals.md](Docs/agents/design-goals.md) explains each goal, the code rules and the checks before a commit.

## Project constraints

- **Mirror the real API per target framework, and nothing more.** `IFileSystem`, `ITimeSystem` and `IRandomSystem` follow the shape of `System.IO`, `DateTime`/timers/`TimeProvider` and `Random`/`Guid` for each target framework. `Testably.Abstractions.Parity.Tests` fails when a member is missing; intentional gaps are excluded in `TestHelpers/Parity.cs` or a `Net*ParityTests` class. Keep the rest of the public API minimal: anything not meant for users is `internal`.
- **The mock must behave like the real file system** on Windows, Linux and macOS, and on .NET Framework where it differs. Use the mock's own context instead of the host: branch on the mock's `Execute` (`IsWindows`, `IsLinux`, `IsMac`, `IsNetFramework`), which honours `SimulationMode`, resolve paths with `Execute.Path`, and create exceptions in `ExceptionFactory`. Check behaviour against the real file system, not against assumptions.
- **Core and main packages are versioned separately.** Core = `Testably.Abstractions`, `Testably.Abstractions.Interface`, `Testably.Abstractions.FileSystem.Interface`, released from `core/v*` tags. Main = `Testably.Abstractions.Testing`, `.Compression`, `.AccessControl`, `.MemoryMappedFiles`, released from `v*` tags. Main projects consume `Testably.Abstractions.Interface` as a NuGet package (version range in `Directory.Packages.props`), so an interface change is invisible to them until a Core release is published and the range is bumped. For such a change set `BuildScope` in `Pipeline/Build.cs` to `CoreOnly`, release Core, bump the range, then reset it (`MainOnly` covers the reverse case).
- **Public API is reviewed.** Every public change shows up in the `*.awexpect-snapshot.txt` files under `Tests/Api/`; update them deliberately and keep them in the same PR. New public members need XML docs (`GenerateDocumentationFile` is on and warnings are errors).
- `netstandard2.0` sources have no polyfill package: hand-written polyfills live in `Source/Testably.Abstractions.Testing/Polyfills/`, and records, `init` and `Index`/`Range` (`^1`, `..`) only compile behind a `FEATURE_*` or `NET*` guard there. Test projects use the `Polyfill` package and may use them on `net48`.

## Validation

- Tests use **TUnit on Microsoft.Testing.Platform** with aweXpect assertions and Mockolate. Test projects are executables; run them with `dotnet test --project <path>`.
- Run only the relevant project and framework: `dotnet test --project Tests/Testably.Abstractions.Tests -f net10.0 --treenode-filter "/*/*/ExistsTests/*"`. The full suite is large; leave it to CI.
- File system tests carry `[FileSystemTests]` and run against `MockFileSystem`, simulated Linux/macOS/Windows mocks (not on `net48`) and the `RealFileSystem`. In Debug the real file system rows are off by default (`Tests/Helpers/Testably.Abstractions.TestHelpers/Settings.cs`); Release and CI run them. A behaviour change is only verified once it passes against the real file system on every OS; CI covers Windows, Linux and macOS.
- Guard OS- or framework-specific expectations with `Skip.If(...)`/`Skip.IfNot(...)` on `Test.RunsOnWindows`, `RunsOnLinux`, `RunsOnMac` or `IsNetFramework`, and assert the behaviour of each platform rather than skipping the mock.
- Public API changes: build the solution, run `Tests/Api/Testably.Abstractions.Api.Tests` and `Tests/Api/Testably.Abstractions.Core.Api.Tests`, and if the change is intended run the explicit `ApiApprovalTests.AcceptApiChanges` test of the affected project to rewrite the snapshots.
- New parity-relevant APIs: run `Tests/Testably.Abstractions.Parity.Tests` on each affected framework.
- Encoding and formatting: tabs, CRLF in the working tree (`.editorconfig`), LF in the index (`.gitattributes`). Most `.cs` files start with a UTF-8 BOM, some do not; keep each file's existing BOM state. PowerShell rewrites change it, so prefer targeted edits and check `git diff` for a changed first line (U+FEFF).
- Do not add em-dashes to docs, XML docs or other Markdown.

## Documentation

Pages under `Docs/pages/docs` are published on docs.testably.org and follow the [docs style guide](https://github.com/Testably/Testably.Site/blob/main/Docs/style-guide.md). The key rules:

- US English, sentence case for headings and labels, no em dashes, one line per paragraph.
- A `description` in the front matter of every page; the H1 is the title; order with unique `NN-` prefixes.
- Relative file links within the repository, site-absolute `/Library/...` links across libraries.
- Install instructions only through `<PackageInstall package="..." />`, never hand-written commands or NuGet badges.
- Examples check results with `await That(...)`; complete examples use TUnit `[Test]`.

The site renames each `00-index.mdx` to `index.mdx` and rewrites relative links to it, so link to a folder's landing page like to any other file (e.g. `./03-file-system/00-index.mdx`). A folder's `_category_.json` sets only its `label`; the prefix orders it.

## Git workflow

- Work on a feature branch and open a pull request against `main`; PRs are squash-merged and the PR title becomes the commit subject.
- PR titles and commit subjects use conventional commit prefixes (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `ci:`, `chore:` ...; see `CONTRIBUTING.md`). The body explains why the change is needed; write each paragraph on a single line and add no co-author or attribution trailers.
- CI (`.github/workflows/ci.yml`) runs unit tests on Ubuntu, Windows and macOS, the API tests and SonarCloud. Sonar issues must be resolved and coverage must stay above 90%.
- Never commit `Artifacts/`, `TestResults/` or local changes to the test `Settings` defaults.

## References

- [`Docs/agents/design-goals.md`](Docs/agents/design-goals.md): the goals and code rules for every change and the checks before a commit.
- [`Docs/agents/workflows.md`](Docs/agents/workflows.md): build, test, API, mutation and documentation commands, pipeline targets, CI and release workflows.
- [`Docs/agents/project-structure.md`](Docs/agents/project-structure.md): projects, architecture of the mocks and where things live.
- [`Docs/agents/testing.md`](Docs/agents/testing.md): writing tests against the real and the mock file system, test settings and OS-specific behaviour.
- [`CONTRIBUTING.md`](CONTRIBUTING.md) and [`Docs/Maintainer.md`](Docs/Maintainer.md): contributor rules and design notes on the mocks.
