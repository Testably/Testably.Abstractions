using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests;

public class MockFileSystemInitializationTests
{
#if CAN_SIMULATE_OTHER_OS
	[SkippableFact]
	public void MockFileSystem_WhenSimulatingLinux_ShouldBeLinux()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(SimulationMode.Linux));

		sut.Execute.IsLinux.Should().BeTrue();
		sut.Execute.IsMac.Should().BeFalse();
		sut.Execute.IsWindows.Should().BeFalse();
		sut.Execute.IsNetFramework.Should().BeFalse();
	}
#endif

#if CAN_SIMULATE_OTHER_OS
	[SkippableFact]
	public void MockFileSystem_WhenSimulatingMacOS_ShouldBeMac()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(SimulationMode.MacOS));

		sut.Execute.IsLinux.Should().BeFalse();
		sut.Execute.IsMac.Should().BeTrue();
		sut.Execute.IsWindows.Should().BeFalse();
		sut.Execute.IsNetFramework.Should().BeFalse();
	}
#endif

#if CAN_SIMULATE_OTHER_OS
	[SkippableFact]
	public void MockFileSystem_WhenSimulatingWindows_ShouldBeWindows()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(SimulationMode.Windows));

		sut.Execute.IsLinux.Should().BeFalse();
		sut.Execute.IsMac.Should().BeFalse();
		sut.Execute.IsWindows.Should().BeTrue();
		sut.Execute.IsNetFramework.Should().BeFalse();
	}
#endif

	[Fact]
	public void MockFileSystem_WithCurrentDirectory_ShouldInitializeCurrentDirectory()
	{
		string expected = Directory.GetCurrentDirectory();
		MockFileSystem sut = new(o => o.UseCurrentDirectory());

		string result = sut.Directory.GetCurrentDirectory();

		result.Should().Be(expected);
	}

	[Theory]
	[AutoData]
	public void MockFileSystem_WithExplicitCurrentDirectory_ShouldInitializeCurrentDirectory(
		string path)
	{
		string expected = Test.RunsOnWindows ? $"C:\\{path}" : $"/{path}";
		MockFileSystem sut = new(o => o.UseCurrentDirectory(path));

		string result = sut.Directory.GetCurrentDirectory();

		result.Should().Be(expected);
	}

	[Fact]
	public void MockFileSystem_WithoutCurrentDirectory_ShouldUseDefaultDriveAsCurrentDirectory()
	{
		string expected = Test.RunsOnWindows ? "C:\\" : "/";
		MockFileSystem sut = new();

		string result = sut.Directory.GetCurrentDirectory();

		result.Should().Be(expected);
	}

#if CAN_SIMULATE_OTHER_OS
	[Theory]
	[MemberData(nameof(ValidOperatingSystems))]
	public void SimulatingOperatingSystem_ValidOSPlatform_ShouldSetOperatingSystem(
		SimulationMode simulationMode)
	{
		MockFileSystem.MockFileSystemOptions sut = new();

		MockFileSystem.MockFileSystemOptions result = sut.SimulatingOperatingSystem(simulationMode);

		result.SimulationMode.Should().Be(simulationMode);
		sut.SimulationMode.Should().Be(simulationMode);
	}
#endif

	[Fact]
	public void UseCurrentDirectory_Empty_ShouldUseCurrentDirectory()
	{
		string expected = Directory.GetCurrentDirectory();
		MockFileSystem.MockFileSystemOptions sut = new();

		MockFileSystem.MockFileSystemOptions result = sut.UseCurrentDirectory();

		result.CurrentDirectory.Should().Be(expected);
		sut.CurrentDirectory.Should().Be(expected);
	}

	[Theory]
	[AutoData]
	public void UseCurrentDirectory_WithPath_ShouldUsePathCurrentDirectory(string path)
	{
		MockFileSystem.MockFileSystemOptions sut = new();

		MockFileSystem.MockFileSystemOptions result = sut.UseCurrentDirectory(path);

		result.CurrentDirectory.Should().Be(path);
		sut.CurrentDirectory.Should().Be(path);
	}

	[Theory]
	[AutoData]
	public void UseRandomProvider_ShouldUseFixedRandomValue(int fixedRandomValue)
	{
		MockFileSystem fileSystem = new(i => i
			.UseRandomProvider(RandomProvider.Generate(
				intGenerator: new RandomProvider.Generator<int>(() => fixedRandomValue))));

		List<int> results = Enumerable.Range(1, 100)
			.Select(_ => fileSystem.RandomSystem.Random.New().Next())
			.ToList();
		results.Add(fileSystem.RandomSystem.Random.Shared.Next());

		results.Should().AllBeEquivalentTo(fixedRandomValue);
	}

	#region Helpers

#if CAN_SIMULATE_OTHER_OS
	public static TheoryData<SimulationMode> ValidOperatingSystems()
		=> new(SimulationMode.Linux, SimulationMode.MacOS, SimulationMode.Windows);
#endif

	#endregion
}
