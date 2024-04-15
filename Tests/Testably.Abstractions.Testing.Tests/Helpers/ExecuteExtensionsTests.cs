using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class ExecuteExtensionsTests
{
	[Theory]
	[InlineData(ExecuteType.Linux, true)]
	[InlineData(ExecuteType.Mac, true)]
	[InlineData(ExecuteType.NetFramework, false)]
	[InlineData(ExecuteType.Windows, true)]
	public void NotOnNetFramework_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.NotOnNetFramework(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, true)]
	[InlineData(ExecuteType.Mac, true)]
	[InlineData(ExecuteType.NetFramework, false)]
	[InlineData(ExecuteType.Windows, false)]
	public void NotOnWindows_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.NotOnWindows(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, false, false)]
	[InlineData(ExecuteType.Linux, true, true)]
	[InlineData(ExecuteType.Mac, false, false)]
	[InlineData(ExecuteType.Mac, true, true)]
	[InlineData(ExecuteType.NetFramework, false, false)]
	[InlineData(ExecuteType.NetFramework, true, false)]
	[InlineData(ExecuteType.Windows, false, false)]
	[InlineData(ExecuteType.Windows, true, false)]
	public void NotOnWindowsIf_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.NotOnWindowsIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, true)]
	[InlineData(ExecuteType.Mac, false)]
	[InlineData(ExecuteType.NetFramework, false)]
	[InlineData(ExecuteType.Windows, false)]
	public void OnLinux_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnLinux(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, 3, 4, 3)]
	[InlineData(ExecuteType.Mac, 5, 6, 6)]
	[InlineData(ExecuteType.NetFramework, 7, 8, 8)]
	[InlineData(ExecuteType.Windows, 1, 2, 2)]
	public void OnLinux_WithValue_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, int value, int alternativeValue, int expectedValue)
	{
		Execute sut = FromType(type);

		int result = sut.OnLinux(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, false)]
	[InlineData(ExecuteType.Mac, true)]
	[InlineData(ExecuteType.NetFramework, false)]
	[InlineData(ExecuteType.Windows, false)]
	public void OnMac_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnMac(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, false)]
	[InlineData(ExecuteType.Mac, false)]
	[InlineData(ExecuteType.NetFramework, true)]
	[InlineData(ExecuteType.Windows, false)]
	public void OnNetFramework_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnNetFramework(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, 3, 4, 4)]
	[InlineData(ExecuteType.Mac, 5, 6, 6)]
	[InlineData(ExecuteType.NetFramework, 7, 8, 7)]
	[InlineData(ExecuteType.Windows, 1, 2, 2)]
	public void OnNetFramework_WithValue_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, int value, int alternativeValue, int expectedValue)
	{
		Execute sut = FromType(type);

		int result = sut.OnNetFramework(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, false, false)]
	[InlineData(ExecuteType.Linux, true, false)]
	[InlineData(ExecuteType.Mac, false, false)]
	[InlineData(ExecuteType.Mac, true, false)]
	[InlineData(ExecuteType.NetFramework, false, false)]
	[InlineData(ExecuteType.NetFramework, true, true)]
	[InlineData(ExecuteType.Windows, false, false)]
	[InlineData(ExecuteType.Windows, true, false)]
	public void OnNetFrameworkIf_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnNetFrameworkIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, false)]
	[InlineData(ExecuteType.Mac, false)]
	[InlineData(ExecuteType.NetFramework, true)]
	[InlineData(ExecuteType.Windows, true)]
	public void OnWindows_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnWindows(() => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, 3, 4, 4)]
	[InlineData(ExecuteType.Mac, 5, 6, 6)]
	[InlineData(ExecuteType.NetFramework, 7, 8, 7)]
	[InlineData(ExecuteType.Windows, 1, 2, 1)]
	public void OnWindows_WithValue_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, int value, int alternativeValue, int expectedValue)
	{
		Execute sut = FromType(type);

		int result = sut.OnWindows(() => value, () => alternativeValue);

		result.Should().Be(expectedValue);
	}

	[Theory]
	[InlineData(ExecuteType.Linux, false, false)]
	[InlineData(ExecuteType.Linux, true, false)]
	[InlineData(ExecuteType.Mac, false, false)]
	[InlineData(ExecuteType.Mac, true, false)]
	[InlineData(ExecuteType.NetFramework, false, false)]
	[InlineData(ExecuteType.NetFramework, true, true)]
	[InlineData(ExecuteType.Windows, false, false)]
	[InlineData(ExecuteType.Windows, true, true)]
	public void OnWindowsIf_ShouldExecuteDependingOnOperatingSystem(
		ExecuteType type, bool predicate, bool shouldExecute)
	{
		bool isExecuted = false;
		Execute sut = FromType(type);

		sut.OnWindowsIf(predicate, () => { isExecuted = true; });

		isExecuted.Should().Be(shouldExecute);
	}

	#region Helpers

	private static Execute FromType(ExecuteType type)
		=> type switch
		{
			ExecuteType.Windows => new Execute(new MockFileSystem(), SimulationMode.Windows),
			ExecuteType.Linux => new Execute(new MockFileSystem(), SimulationMode.Linux),
			ExecuteType.Mac => new Execute(new MockFileSystem(), SimulationMode.MacOS),
			ExecuteType.NetFramework => new Execute(new MockFileSystem(), SimulationMode.Windows, true),
			_ => throw new NotSupportedException()
		};

	#endregion

	public enum ExecuteType
	{
		Windows,
		Linux,
		Mac,
		NetFramework
	}
}
