using System.IO;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public sealed class PathMockTests
{
#if CAN_SIMULATE_OTHER_OS
	[Theory]
	[InlineAutoData(SimulationMode.Native)]
	[InlineAutoData(SimulationMode.Linux)]
	[InlineAutoData(SimulationMode.MacOS)]
	[InlineAutoData(SimulationMode.Windows)]
	public void GetTempFileName_WithCollisions_ShouldThrowIOException(
		SimulationMode simulationMode, int fixedRandomValue)
	{
		MockFileSystem fileSystem = new(o => o
			.SimulatingOperatingSystem(simulationMode)
			.UseRandomProvider(RandomProvider.Generate(
				intGenerator: new RandomProvider.Generator<int>(() => fixedRandomValue))));
		#pragma warning disable CS0618
		string result = fileSystem.Path.GetTempFileName();

		Exception? exception = Record.Exception(() =>
		{
			_ = fileSystem.Path.GetTempFileName();
		});
		#pragma warning restore CS0618

		exception.Should().BeOfType<IOException>();
		fileSystem.File.Exists(result).Should().BeTrue();
	}
#else
	[Theory]
	[AutoData]
	public void GetTempFileName_WithCollisions_ShouldThrowIOException(
		int fixedRandomValue)
	{
		MockFileSystem fileSystem = new(o => o
			.UseRandomProvider(RandomProvider.Generate(
				intGenerator: new RandomProvider.Generator<int>(() => fixedRandomValue))));
		string result = fileSystem.Path.GetTempFileName();

		Exception? exception = Record.Exception(() =>
		{
			_ = fileSystem.Path.GetTempFileName();
		});

		exception.Should().BeOfType<IOException>();
		fileSystem.File.Exists(result).Should().BeTrue();
	}
#endif
}
