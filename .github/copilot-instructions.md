# GitHub Copilot Instructions for Testably.Abstractions

> **IMPORTANT: Follow these instructions FIRST before searching or exploring the repository. These have been exhaustively validated and contain critical timing and setup information.**

## Project Overview

Testably.Abstractions is a feature-complete testing helper for the `System.IO.Abstractions` library. It provides an in-memory file system that behaves exactly like the real file system and can be used in unit tests for dependency injection.

### Key Features
- Mock file system with identical behavior to real file system
- Cross-platform testing (Linux, macOS, Windows simulation)
- Advanced scenarios: multiple drives, FileSystemWatcher, SafeFileHandles
- Companion projects for Compression and AccessControl
- Time and Random system abstractions

### Architecture
- **Source/**: Main library code with 6 projects
- **Tests/**: Comprehensive test suite with 13,134+ tests
- **Examples/**: Usage examples and documentation
- **Pipeline/**: Nuke build system with .NET 8.0

## Critical Setup Requirements

### 1. .NET SDK Installation
**NEVER CANCEL** the .NET SDK download - it takes 45+ seconds but is REQUIRED:
```bash
# .NET 9.0.303 SDK will auto-download via build.sh
# .NET 8.0 runtime is also automatically installed for build system
# This process can take 45+ seconds - DO NOT CANCEL
```

### 2. Git Repository Setup
**CRITICAL**: Repository must be unshallowed for versioning to work:
```bash
git fetch --unshallow
```

## Working Commands (All Exhaustively Validated)

### Build Commands
**Build Time: ~45 seconds (set timeout: 240s)**
```bash
# Full build (Debug mode)
./build.sh --target Compile

# Release build for packaging
export PATH="./.nuke/temp/dotnet-unix:$PATH"
dotnet build --configuration Release
```

### Test Commands
**Test Time: ~50 seconds for 13,134 tests (set timeout: 120s)**
```bash
# Run all tests
export PATH="./.nuke/temp/dotnet-unix:$PATH"
dotnet test --no-build

# Tests run on both MockFileSystem and RealFileSystem
# Expected: ~19,355 succeeded, ~7,333 skipped (platform-specific)
```

### Package Commands
**Package Time: ~3 seconds (set timeout: 60s)**
```bash
# Create NuGet packages (6 packages total)
export PATH="./.nuke/temp/dotnet-unix:$PATH"
dotnet pack --no-build --configuration Release
```

### Build System Help
```bash
# View all available Nuke targets
./build.sh --help

# Available targets:
# - Compile: Build all projects
# - UnitTests: Run unit tests  
# - Pack: Create NuGet packages
# - CodeCoverage: Generate coverage reports
# - ApiChecks: Validate API surface
```

## Important Limitations

### GitVersion Issues
- **Known Issue**: GitVersion fails on feature branches due to orphaned branch detection
- **Workaround**: Use plain `dotnet build/test/pack` instead of Nuke targets for versioning-dependent operations
- **Alternative**: Build with `--target Compile` works for compilation only

### Platform Dependencies
- Some tests require Windows-specific features (skipped on Linux/macOS)
- Mono dependency for .NET Framework 4.8 tests
- Encryption tests depend on underlying device support

### Build Warnings
- System.Threading.Channels warnings on .NET 6.0 are expected and safe
- SonarCloud integration requires SONAR_TOKEN environment variable
- GitVersion warnings on feature branches are expected

## Validation Scenarios

### After Making Changes
1. **Build Test**:
   ```bash
   time ./build.sh --target Compile  # Should complete in ~45s
   ```

2. **Quick Test**:
   ```bash
   export PATH="./.nuke/temp/dotnet-unix:$PATH"
   dotnet test --no-build --filter "TestCategory!=LongRunning"
   ```

3. **Full Test**:
   ```bash
   export PATH="./.nuke/temp/dotnet-unix:$PATH"
   time dotnet test --no-build  # Should complete in ~50s
   ```

### Expected Outputs
- **Build Success**: "Build succeeded with X warning(s)"
- **Test Success**: "Test summary: total: 26699, failed: 0, succeeded: 19355, skipped: 7333"
- **Package Success**: 6 NuGet packages created in Release configuration

## Project Structure Guide

### Core Libraries (Source/)
- `Testably.Abstractions`: Main abstraction interfaces
- `Testably.Abstractions.Testing`: Mock implementations
- `Testably.Abstractions.Interface`: Core interfaces
- `Testably.Abstractions.FileSystem.Interface`: File system interfaces
- `Testably.Abstractions.Compression`: Zip file support
- `Testably.Abstractions.AccessControl`: ACL support

### Test Projects (Tests/)
- `Testably.Abstractions.Tests`: Main test suite (~20,000+ tests)
- `Testably.Abstractions.Parity.Tests`: Real vs Mock parity tests
- `Testably.Abstractions.Testing.Tests`: Mock framework tests
- Platform-specific test configurations for .NET 4.8, 6.0, 8.0, 9.0

### Configuration Files
- `global.json`: .NET SDK version (9.0.303)
- `Directory.Build.props`: Common MSBuild properties
- `Feature.Flags.props`: Feature toggles
- `Tests/Settings/`: Test configuration settings

## Common Tasks

### Adding New Tests
1. Choose appropriate test project in `Tests/`
2. Follow existing patterns for Mock vs Real testing
3. Use `Test.RunsOn()` attributes for platform-specific tests
4. Consider both `MockFileSystem` and `RealFileSystem` scenarios

### Debugging Failed Tests
1. Check if test is platform-specific (Linux vs Windows behavior)
2. Verify test settings in `Tests/Settings/test.settings.json`
3. Use `--filter` to isolate specific test categories
4. Check for timing-sensitive tests in LongRunning category

### Performance Testing
- Most tests complete quickly (<1ms per test average)
- Long-running tests are in separate category
- File system operations are in-memory for mock tests
- Real file system tests may take longer due to actual I/O

### Pull Request Title
To communicate intent to the consumers of your library, the title of the pull requests is prefixed with one of the following elements:
- `fix:`: patches a bug
- `feat:`: introduces a new feature
- `refactor:`: improves internal structure without changing the observable behavior
- `docs:`: updates documentation or XML comments
- `chore:`: updates to dependencies, build pipelines, ...

## Critical Warnings

⚠️ **NEVER CANCEL** long-running operations:
- .NET SDK download (45+ seconds)
- Full test suite (50+ seconds)
- Initial build compilation (45+ seconds)

⚠️ **ALWAYS** set appropriate timeouts:
- Build operations: 240 seconds
- Test operations: 120 seconds
- Package operations: 60 seconds

⚠️ **REQUIRED** environment setup:
- `git fetch --unshallow` before first build
- Export PATH for dotnet commands
- Use Release configuration for packaging

This comprehensive guide ensures successful development workflow in the Testably.Abstractions codebase with proper understanding of timing requirements and platform limitations.
