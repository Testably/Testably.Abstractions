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
	public async Task GetTempFileName_WithCollisions_ShouldThrowIOException(
		SimulationMode simulationMode, int fixedRandomValue)
	{
		MockFileSystem fileSystem = new(o => o
			.SimulatingOperatingSystem(simulationMode)
			.UseRandomProvider(RandomProvider.Generate(
				intGenerator: new RandomProvider.Generator<int>(() => fixedRandomValue))));
#pragma warning disable CS0618
		string result = fileSystem.Path.GetTempFileName();
		
		void Act()
		{
			_ = fileSystem.Path.GetTempFileName();
		}
#pragma warning restore CS0618


		await That(Act).ThrowsExactly<IOException>();
		await That(fileSystem.File.Exists(result)).IsTrue();
	}
#else
	[Theory]
	[AutoData]
	public async Task GetTempFileName_WithCollisions_ShouldThrowIOException(
		int fixedRandomValue)
	{
		MockFileSystem fileSystem = new(o => o
			.UseRandomProvider(RandomProvider.Generate(
				intGenerator: new RandomProvider.Generator<int>(() => fixedRandomValue))));
		string result = fileSystem.Path.GetTempFileName();

		void Act()
		{
			_ = fileSystem.Path.GetTempFileName();
		}

		await That(Act).ThrowsExactly<IOException>();
		await That(fileSystem.File.Exists(result)).IsTrue();
	}
#endif
}
