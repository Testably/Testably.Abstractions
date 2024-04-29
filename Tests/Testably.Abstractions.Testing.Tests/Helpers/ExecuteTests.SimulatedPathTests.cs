namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed partial class ExecuteTests
{
	public sealed class SimulatedPathTests
	{
#if CAN_SIMULATE_OTHER_OS
		[Theory]
		[InlineData(SimulationMode.Linux)]
		[InlineData(SimulationMode.MacOS)]
		[InlineData(SimulationMode.Windows)]
		public void FileSystem_ShouldBeMockFileSystem(SimulationMode simulationMode)
		{
			MockFileSystem sut = new(o => o.SimulatingOperatingSystem(simulationMode));

			sut.Execute.Path.FileSystem.Should().BeSameAs(sut);
		}
#endif
	}
}
