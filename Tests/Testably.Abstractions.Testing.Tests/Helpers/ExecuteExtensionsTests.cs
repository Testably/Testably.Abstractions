using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class ExecuteExtensionsTests
{
	[Theory]
	[InlineData(SimulationMode.Linux, true)]
	[InlineData(SimulationMode.MacOS, true)]
	[InlineData(SimulationMode.Windows, true)]
	public void NotOnNetFramework_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.NotOnNetFramework(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, true)]
	[InlineData(SimulationMode.MacOS, true)]
	[InlineData(SimulationMode.Windows, false)]
	public void NotOnWindows_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.NotOnWindows(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false)]
	[InlineData(SimulationMode.Linux, true, true)]
	[InlineData(SimulationMode.MacOS, false, false)]
	[InlineData(SimulationMode.MacOS, true, true)]
	[InlineData(SimulationMode.Windows, false, false)]
	[InlineData(SimulationMode.Windows, true, false)]
	public void NotOnWindowsIf_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.NotOnWindowsIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, true)]
	[InlineData(SimulationMode.MacOS, false)]
	[InlineData(SimulationMode.Windows, false)]
	public void OnLinux_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnLinux(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, 3, 4, 3)]
	[InlineData(SimulationMode.MacOS, 5, 6, 6)]
	[InlineData(SimulationMode.Windows, 1, 2, 2)]
	public void OnLinux_WithValue_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, int value, int alternativeValue, int expectedValue)
	{
		Execute sut = FromType(type);

		int result = sut.OnLinux(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false)]
	[InlineData(SimulationMode.MacOS, true)]
	[InlineData(SimulationMode.Windows, false)]
	public void OnMac_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnMac(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false)]
	[InlineData(SimulationMode.MacOS, false)]
	[InlineData(SimulationMode.Windows, false)]
	public void OnNetFramework_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnNetFramework(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, 3, 4, 4)]
	[InlineData(SimulationMode.MacOS, 5, 6, 6)]
	[InlineData(SimulationMode.Windows, 1, 2, 2)]
	public void OnNetFramework_WithValue_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, int value, int alternativeValue, int expectedValue)
	{
		Execute sut = FromType(type);

		int result = sut.OnNetFramework(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false)]
	[InlineData(SimulationMode.Linux, true, false)]
	[InlineData(SimulationMode.MacOS, false, false)]
	[InlineData(SimulationMode.MacOS, true, false)]
	[InlineData(SimulationMode.Windows, false, false)]
	[InlineData(SimulationMode.Windows, true, false)]
	public void OnNetFrameworkIf_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnNetFrameworkIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false)]
	[InlineData(SimulationMode.MacOS, false)]
	[InlineData(SimulationMode.Windows, true)]
	public void OnWindows_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnWindows(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, 3, 4, 4)]
	[InlineData(SimulationMode.MacOS, 5, 6, 6)]
	[InlineData(SimulationMode.Windows, 1, 2, 1)]
	public void OnWindows_WithValue_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, int value, int alternativeValue, int expectedValue)
	{
		Execute sut = FromType(type);

		int result = sut.OnWindows(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false)]
	[InlineData(SimulationMode.Linux, true, false)]
	[InlineData(SimulationMode.MacOS, false, false)]
	[InlineData(SimulationMode.MacOS, true, false)]
	[InlineData(SimulationMode.Windows, false, false)]
	[InlineData(SimulationMode.Windows, true, true)]
	public void OnWindowsIf_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnWindowsIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	#region Helpers

	#pragma warning disable CS0618
	private static Execute FromType(SimulationMode type)
		=> new(new MockFileSystem(), type);
	#pragma warning restore CS0618

	#endregion
}
