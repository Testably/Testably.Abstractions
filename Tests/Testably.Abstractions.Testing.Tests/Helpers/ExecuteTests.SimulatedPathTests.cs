namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed partial class ExecuteTests
{
#if CAN_SIMULATE_OTHER_OS
	public sealed class SimulatedPathTests
	{
		[Test]
		[Arguments(SimulationMode.Linux)]
		[Arguments(SimulationMode.MacOS)]
		[Arguments(SimulationMode.Windows)]
		public async Task FileSystem_ShouldBeMockFileSystem(SimulationMode simulationMode)
		{
			MockFileSystem sut = new(o => o.SimulatingOperatingSystem(simulationMode));

			await That(sut.Execute.Path.FileSystem).IsSameAs(sut);
		}
	}
#endif
}
