using System.IO;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public sealed class PathMockTests
{
	[Theory]
	[InlineAutoData(SimulationMode.Native)]
	[InlineAutoData(SimulationMode.Linux)]
	[InlineAutoData(SimulationMode.MacOS)]
	[InlineAutoData(SimulationMode.Windows)]
	public void GetTempFileName_WithCollisions_ShouldThrowIOException(
		SimulationMode simulationMode, int fixedRandomValue)
	{
		MockFileSystem fileSystem = new(i => i
			.SimulatingOperatingSystem(simulationMode)
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
}
