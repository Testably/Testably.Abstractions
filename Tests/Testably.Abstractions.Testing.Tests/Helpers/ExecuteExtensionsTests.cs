using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class ExecuteExtensionsTests
{
	[Theory]
	[InlineData(SimulationMode.Linux, false, true)]
	[InlineData(SimulationMode.MacOS, false, true)]
	[InlineData(SimulationMode.Windows, false, true)]
	[InlineData(SimulationMode.Linux, true, true)]
	[InlineData(SimulationMode.MacOS, true, true)]
	[InlineData(SimulationMode.Windows, true, false)]
	public void NotOnNetFramework_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.NotOnNetFramework(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, true)]
	[InlineData(SimulationMode.MacOS, false, true)]
	[InlineData(SimulationMode.Windows, false, false)]
	[InlineData(SimulationMode.Windows, true, false)]
	public void NotOnWindows_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.NotOnWindows(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false, false)]
	[InlineData(SimulationMode.Linux, false, true, true)]
	[InlineData(SimulationMode.MacOS, false, false, false)]
	[InlineData(SimulationMode.MacOS, false, true, true)]
	[InlineData(SimulationMode.Windows, false, false, false)]
	[InlineData(SimulationMode.Windows, false, true, false)]
	[InlineData(SimulationMode.Windows, true, false, false)]
	[InlineData(SimulationMode.Windows, true, true, false)]
	public void NotOnWindowsIf_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.NotOnWindowsIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, true)]
	[InlineData(SimulationMode.MacOS, false, false)]
	[InlineData(SimulationMode.Windows, false, false)]
	public void OnLinux_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.OnLinux(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, 3, 4, 3)]
	[InlineData(SimulationMode.MacOS, false, 5, 6, 6)]
	[InlineData(SimulationMode.Windows, false, 1, 2, 2)]
	public void OnLinux_WithValue_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type,
		bool isNetFramework,
		int value,
		int alternativeValue,
		int expectedValue)
	{
		Execute sut = FromType(type, isNetFramework);

		int result = sut.OnLinux(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false)]
	[InlineData(SimulationMode.MacOS, false, true)]
	[InlineData(SimulationMode.Windows, false, false)]
	public void OnMac_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.OnMac(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false)]
	[InlineData(SimulationMode.MacOS, false, false)]
	[InlineData(SimulationMode.Windows, false, false)]
	[InlineData(SimulationMode.Linux, true, false)]
	[InlineData(SimulationMode.MacOS, true, false)]
	[InlineData(SimulationMode.Windows, true, true)]
	public void OnNetFramework_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.OnNetFramework(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, 3, 4, 4)]
	[InlineData(SimulationMode.MacOS, false, 5, 6, 6)]
	[InlineData(SimulationMode.Windows, false, 1, 2, 2)]
	[InlineData(SimulationMode.Linux, true, 3, 4, 4)]
	[InlineData(SimulationMode.MacOS, true, 5, 6, 6)]
	[InlineData(SimulationMode.Windows, true, 1, 2, 1)]
	public void OnNetFramework_WithValue_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type,
		bool isNetFramework,
		int value,
		int alternativeValue,
		int expectedValue)
	{
		Execute sut = FromType(type, isNetFramework);

		int result = sut.OnNetFramework(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false, false)]
	[InlineData(SimulationMode.Linux, false, true, false)]
	[InlineData(SimulationMode.MacOS, false, false, false)]
	[InlineData(SimulationMode.MacOS, false, true, false)]
	[InlineData(SimulationMode.Windows, false, false, false)]
	[InlineData(SimulationMode.Windows, false, true, false)]
	[InlineData(SimulationMode.Linux, true, false, false)]
	[InlineData(SimulationMode.Linux, true, true, false)]
	[InlineData(SimulationMode.MacOS, true, false, false)]
	[InlineData(SimulationMode.MacOS, true, true, false)]
	[InlineData(SimulationMode.Windows, true, false, false)]
	[InlineData(SimulationMode.Windows, true, true, true)]
	public void OnNetFrameworkIf_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.OnNetFrameworkIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false)]
	[InlineData(SimulationMode.MacOS, false, false)]
	[InlineData(SimulationMode.Windows, false, true)]
	[InlineData(SimulationMode.Windows, true, true)]
	public void OnWindows_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.OnWindows(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, 3, 4, 4)]
	[InlineData(SimulationMode.MacOS, false, 5, 6, 6)]
	[InlineData(SimulationMode.Windows, false, 1, 2, 1)]
	[InlineData(SimulationMode.Windows, true, 1, 2, 1)]
	public void OnWindows_WithValue_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type,
		bool isNetFramework,
		int value,
		int alternativeValue,
		int expectedValue)
	{
		Execute sut = FromType(type, isNetFramework);

		int result = sut.OnWindows(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(SimulationMode.Linux, false, false, false)]
	[InlineData(SimulationMode.Linux, false, true, false)]
	[InlineData(SimulationMode.MacOS, false, false, false)]
	[InlineData(SimulationMode.MacOS, false, true, false)]
	[InlineData(SimulationMode.Windows, false, false, false)]
	[InlineData(SimulationMode.Windows, false, true, true)]
	[InlineData(SimulationMode.Windows, true, false, false)]
	[InlineData(SimulationMode.Windows, true, true, true)]
	public void OnWindowsIf_ShouldExecuteDependingOnOperatingSystem(
		SimulationMode type, bool isNetFramework, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type, isNetFramework);

		sut.OnWindowsIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	#region Helpers

	#pragma warning disable CS0618
	private static Execute FromType(SimulationMode type, bool isNetFramework)
		=> new(new MockFileSystem(), type, isNetFramework);
	#pragma warning restore CS0618

	#endregion
}
