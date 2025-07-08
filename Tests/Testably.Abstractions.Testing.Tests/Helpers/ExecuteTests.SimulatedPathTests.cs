namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed partial class ExecuteTests
{
#if CAN_SIMULATE_OTHER_OS
	public sealed class SimulatedPathTests
	{
		[Theory]
		[InlineData(SimulationMode.Linux)]
		[InlineData(SimulationMode.MacOS)]
		[InlineData(SimulationMode.Windows)]
		public async Task FileSystem_ShouldBeMockFileSystem(SimulationMode simulationMode)
		{
			MockFileSystem sut = new(o => o.SimulatingOperatingSystem(simulationMode));

			await That(sut.Execute.Path.FileSystem).IsSameAs(sut);
		}
	}
#endif
}
