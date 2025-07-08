using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests;

public class MockFileSystemInitializationTests
{
#if CAN_SIMULATE_OTHER_OS
	[Fact]
	public async Task MockFileSystem_WhenSimulatingLinux_ShouldBeLinux()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(SimulationMode.Linux));

		await That(sut.Execute.IsLinux).IsTrue();
		await That(sut.Execute.IsMac).IsFalse();
		await That(sut.Execute.IsWindows).IsFalse();
		await That(sut.Execute.IsNetFramework).IsFalse();
	}
#endif

#if CAN_SIMULATE_OTHER_OS
	[Fact]
	public async Task MockFileSystem_WhenSimulatingMacOS_ShouldBeMac()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(SimulationMode.MacOS));

		await That(sut.Execute.IsLinux).IsFalse();
		await That(sut.Execute.IsMac).IsTrue();
		await That(sut.Execute.IsWindows).IsFalse();
		await That(sut.Execute.IsNetFramework).IsFalse();
	}
#endif

#if CAN_SIMULATE_OTHER_OS
	[Fact]
	public async Task MockFileSystem_WhenSimulatingWindows_ShouldBeWindows()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(SimulationMode.Windows));

		await That(sut.Execute.IsLinux).IsFalse();
		await That(sut.Execute.IsMac).IsFalse();
		await That(sut.Execute.IsWindows).IsTrue();
		await That(sut.Execute.IsNetFramework).IsFalse();
	}
#endif

	[Fact]
	public async Task MockFileSystem_WithCurrentDirectory_ShouldInitializeCurrentDirectory()
	{
		string expected = Directory.GetCurrentDirectory();
		MockFileSystem sut = new(o => o.UseCurrentDirectory());

		string result = sut.Directory.GetCurrentDirectory();

		await That(result).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public async Task MockFileSystem_WithExplicitCurrentDirectory_ShouldInitializeCurrentDirectory(
		string path)
	{
		string expected = Test.RunsOnWindows ? $"C:\\{path}" : $"/{path}";
		MockFileSystem sut = new(o => o.UseCurrentDirectory(path));

		string result = sut.Directory.GetCurrentDirectory();

		await That(result).IsEqualTo(expected);
	}

	[Fact]
	public async Task MockFileSystem_WithoutCurrentDirectory_ShouldUseDefaultDriveAsCurrentDirectory()
	{
		string expected = Test.RunsOnWindows ? "C:\\" : "/";
		MockFileSystem sut = new();

		string result = sut.Directory.GetCurrentDirectory();

		await That(result).IsEqualTo(expected);
	}

#if CAN_SIMULATE_OTHER_OS
	[Theory]
	[MemberData(nameof(ValidOperatingSystems))]
	public async Task SimulatingOperatingSystem_ValidOSPlatform_ShouldSetOperatingSystem(
		SimulationMode simulationMode)
	{
		MockFileSystem.MockFileSystemOptions sut = new();

		MockFileSystem.MockFileSystemOptions result = sut.SimulatingOperatingSystem(simulationMode);

		await That(result.SimulationMode).IsEqualTo(simulationMode);
		await That(sut.SimulationMode).IsEqualTo(simulationMode);
	}
#endif

	[Fact]
	public async Task UseCurrentDirectory_Empty_ShouldUseCurrentDirectory()
	{
		string expected = Directory.GetCurrentDirectory();
		MockFileSystem.MockFileSystemOptions sut = new();

		MockFileSystem.MockFileSystemOptions result = sut.UseCurrentDirectory();

		await That(result.CurrentDirectory).IsEqualTo(expected);
		await That(sut.CurrentDirectory).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public async Task UseCurrentDirectory_WithPath_ShouldUsePathCurrentDirectory(string path)
	{
		MockFileSystem.MockFileSystemOptions sut = new();

		MockFileSystem.MockFileSystemOptions result = sut.UseCurrentDirectory(path);

		await That(result.CurrentDirectory).IsEqualTo(path);
		await That(sut.CurrentDirectory).IsEqualTo(path);
	}

	[Theory]
	[AutoData]
	public async Task UseRandomProvider_ShouldUseFixedRandomValue(int fixedRandomValue)
	{
		MockFileSystem fileSystem = new(i => i
			.UseRandomProvider(RandomProvider.Generate(
				intGenerator: new RandomProvider.Generator<int>(() => fixedRandomValue))));

		List<int> results = Enumerable.Range(1, 100)
			.Select(_ => fileSystem.RandomSystem.Random.New().Next())
			.ToList();
		results.Add(fileSystem.RandomSystem.Random.Shared.Next());

		await That(results).All().AreEqualTo(fixedRandomValue);
	}

	#region Helpers

#if CAN_SIMULATE_OTHER_OS
	public static TheoryData<SimulationMode> ValidOperatingSystems()
		=> new(SimulationMode.Linux, SimulationMode.MacOS, SimulationMode.Windows);
#endif

	#endregion
}
