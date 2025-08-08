# aweXpect - .NET Assertion Library

aweXpect is a modern .NET assertion library providing fluent APIs for unit testing. It supports async-first patterns, multiple testing frameworks, and extensibility through a plugin architecture.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap and Build Requirements
- **Required SDK**: .NET 8.0.407 (specified in `global.json`)
- **Build System**: NUKE build automation system
- **Git Requirements**: Full git history required (`git fetch --unshallow` if shallow clone)

### Primary Build Commands
**IMPORTANT**: Use appropriate timeouts for all commands. NEVER CANCEL builds or tests.

#### Standard Build Process (when GitVersion works):
```bash
./build.sh Compile  # Takes 2-3 minutes. NEVER CANCEL. Set timeout to 5+ minutes.
./build.sh UnitTests  # Takes 3-5 minutes. NEVER CANCEL. Set timeout to 10+ minutes.
./build.sh CodeCoverage  # Takes 5-8 minutes. NEVER CANCEL. Set timeout to 15+ minutes.
./build.sh Pack  # Takes 2-3 minutes. NEVER CANCEL. Set timeout to 5+ minutes.
```

#### Fallback Build Process (when GitVersion fails):
When encountering GitVersion issues (common with branch refs), use direct dotnet commands:
```bash
# Use the NUKE-installed SDK for correct version
export DOTNET_PATH="/home/runner/work/aweXpect/aweXpect/.nuke/temp/dotnet-unix/dotnet"

# Build the solution
$DOTNET_PATH build aweXpect.sln --configuration Release  # Takes 1m20s. NEVER CANCEL. Set timeout to 3+ minutes.

# Run tests (.NET 8.0 only to avoid mono dependency issues)
$DOTNET_PATH test aweXpect.sln --configuration Release --no-build --framework net8.0  # Takes 30s. NEVER CANCEL. Set timeout to 2+ minutes.
```

### Available NUKE Build Targets
Use `./build.sh --help` to see all targets. Key targets:
- `ApiChecks` - API compatibility validation
- `Compile` - Full solution build with versioning
- `UnitTests` - Execute all unit tests
- `CodeCoverage` - Unit tests with coverage analysis
- `TestFrameworks` - Framework-specific integration tests
- `Pack` - Create NuGet packages
- `CodeAnalysis` - SonarCloud static analysis
- `MutationTests` - Stryker.NET mutation testing (takes 15+ minutes)
- `Benchmarks` - BenchmarkDotNet performance tests

## Validation and Testing

### Manual Validation Workflow
Always test basic functionality after making changes:

1. **Build validation**:
   ```bash
   ./build.sh Compile  # Or use fallback if GitVersion fails
   ```

2. **Unit test validation**:
   ```bash
   ./build.sh UnitTests  # Or use fallback with --framework net8.0
   ```

3. **API compatibility**:
   ```bash
   ./build.sh ApiChecks  # Validates public API changes
   ```

### Testing Framework Support
The library supports multiple testing frameworks. Test projects are in `Tests/Frameworks/`:
- **NUnit3/NUnit4**: `aweXpect.Frameworks.NUnit*.Tests`
- **xUnit2/xUnit3**: `aweXpect.Frameworks.XUnit*.Tests`
- **MSTest**: `aweXpect.Frameworks.MsTest.Tests`
- **TUnit**: `aweXpect.Frameworks.TUnit.Tests`

### Critical Testing Notes
- **Framework Limitation**: .NET Framework tests require mono (not available on Linux). Use `--framework net8.0` flag.
- **Test Count**: Over 12,000 unit tests. Full test suite takes 5+ minutes.
- **Coverage Requirements**: Maintain >90% code coverage (SonarCloud requirement).

## Known Issues and Workarounds

### Pull Request Title
To communicate intent to the consumers of your library, the title of the pull requests is prefixed with one of the following elements:
- `fix:`: patches a bug
- `feat:`: introduces a new feature
- `refactor`: improves internal structure without changing the observable behavior
- `docs`: updates documentation or XML comments
- `chore`: updates to dependencies, build pipelines, ...

### GitVersion Failures
**Symptom**: Build fails with `LibGit2SharpException: ref doesn't match destination`
**Cause**: Branch reference issues or shallow clone
**Solutions**:
1. Ensure full git history: `git fetch --unshallow`
2. Switch to main branch temporarily for builds
3. Use fallback dotnet commands (documented above)

### Framework Testing Limitations
**Issue**: .NET Framework tests fail with "Could not find 'mono' host"
**Solution**: Focus testing on .NET 8.0 using `--framework net8.0` flag

### Build Performance
- **Restore**: 2-3 minutes on first run, seconds on subsequent runs
- **Compilation**: 1-2 minutes
- **Full test suite**: 3-5 minutes
- **Mutation tests**: 15+ minutes (only run when needed)

## Project Structure

### Core Libraries (Source/)
- **aweXpect**: Main library package
- **aweXpect.Core**: Core functionality for extensions
- **aweXpect.Analyzers**: Roslyn analyzers
- **aweXpect.Analyzers.CodeFixers**: Code fixers

### Test Organization (Tests/)
- **aweXpect.Tests**: Main test suite (12,000+ tests)
- **aweXpect.Core.Tests**: Core library tests
- **aweXpect.Internal.Tests**: Internal functionality tests
- **aweXpect.Api.Tests**: API compatibility tests
- **Frameworks/**: Framework-specific integration tests

### Key Configuration Files
- **global.json**: SDK version requirements (.NET 8.0.407)
- **Directory.Packages.props**: Centralized package management
- **aweXpect.sln**: Main solution file
- **Pipeline/Build.cs**: NUKE build configuration

## Extension Development

### Adding New Functionality
Extend functionality by adding extension methods on `IThat<TType>`:
```csharp
public static AndOrResult<TType, TSubject> BeCustom<TType, TSubject>(
    this IThat<TType> source) 
    where TType : IThat<TType, TSubject>
{
    // Implementation
}
```

### Build Integration
Always validate extensions work with all supported frameworks:
1. Run framework-specific tests
2. Ensure API compatibility with `ApiChecks`
3. Verify performance impact with `Benchmarks`

## Common Development Tasks

### After Making Code Changes
1. **Build**: `./build.sh Compile` (3+ minute timeout)
2. **Test**: `./build.sh UnitTests` (10+ minute timeout)
3. **API Check**: `./build.sh ApiChecks` (5+ minute timeout)
4. **Performance**: `./build.sh Benchmarks` (optional, 10+ minute timeout)

### Before Committing
Always run these validation steps:
1. Full build and test cycle
2. API compatibility validation
3. Ensure all tests pass (12,000+ tests expected)

### CI/CD Pipeline
The `.github/workflows/build.yml` runs:
- Multi-platform builds (Ubuntu, Windows, macOS)
- Comprehensive testing across frameworks
- Static analysis with SonarCloud
- Mutation testing with Stryker.NET
- Package generation and publishing

Remember: **NEVER CANCEL** long-running builds or tests. They are expected to take significant time and the CI pipeline depends on complete execution.
