using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class ExecuteTests
{
	[Fact]
	public void Constructor_ForLinux_ShouldInitializeAccordingly()
	{
		Execute sut = new(new MockFileSystem(), SimulationMode.Linux);

		sut.IsLinux.Should().BeTrue();
		sut.IsMac.Should().BeFalse();
		sut.IsNetFramework.Should().BeFalse();
		sut.IsWindows.Should().BeFalse();
		sut.StringComparisonMode.Should().Be(StringComparison.Ordinal);
	}

	[Fact]
	public void Constructor_ForNetFramework_ShouldInitializeAccordingly()
	{
		Execute sut = new(new MockFileSystem(), SimulationMode.Windows, true);

		sut.IsLinux.Should().BeFalse();
		sut.IsMac.Should().BeFalse();
		sut.IsNetFramework.Should().BeTrue();
		sut.IsWindows.Should().BeTrue();
		sut.StringComparisonMode.Should().Be(StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void Constructor_ForNetFramework_WithLinux_ShouldInitializeLinux()
	{
		Execute sut = new(new MockFileSystem(), SimulationMode.Linux, true);

		sut.IsLinux.Should().BeTrue();
		sut.IsMac.Should().BeFalse();
		sut.IsNetFramework.Should().BeFalse();
		sut.IsWindows.Should().BeFalse();
		sut.StringComparisonMode.Should().Be(StringComparison.Ordinal);
	}

	[Fact]
	public void Constructor_ForNetFramework_WithOSX_ShouldInitializeMac()
	{
		Execute sut = new(new MockFileSystem(), SimulationMode.MacOS, true);

		sut.IsLinux.Should().BeFalse();
		sut.IsMac.Should().BeTrue();
		sut.IsNetFramework.Should().BeFalse();
		sut.IsWindows.Should().BeFalse();
		sut.StringComparisonMode.Should().Be(StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void Constructor_ForOSX_ShouldInitializeAccordingly()
	{
		Execute sut = new(new MockFileSystem(), SimulationMode.MacOS);

		sut.IsLinux.Should().BeFalse();
		sut.IsMac.Should().BeTrue();
		sut.IsNetFramework.Should().BeFalse();
		sut.IsWindows.Should().BeFalse();
		sut.StringComparisonMode.Should().Be(StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void Constructor_ForWindows_ShouldInitializeAccordingly()
	{
		Execute sut = new(new MockFileSystem(), SimulationMode.Windows);

		sut.IsLinux.Should().BeFalse();
		sut.IsMac.Should().BeFalse();
		sut.IsNetFramework.Should().BeFalse();
		sut.IsWindows.Should().BeTrue();
		sut.StringComparisonMode.Should().Be(StringComparison.OrdinalIgnoreCase);
	}
}
