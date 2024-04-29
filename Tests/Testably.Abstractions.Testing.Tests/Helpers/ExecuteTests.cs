using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed partial class ExecuteTests
{
	[Fact]
	public void Constructor_ForLinux_ShouldInitializeAccordingly()
	{
		#pragma warning disable CS0618
		Execute sut = new(new MockFileSystem(), SimulationMode.Linux);
		#pragma warning restore CS0618

		sut.IsLinux.Should().BeTrue();
		sut.IsMac.Should().BeFalse();
		sut.IsNetFramework.Should().BeFalse();
		sut.IsWindows.Should().BeFalse();
		sut.StringComparisonMode.Should().Be(StringComparison.Ordinal);
	}

	[Fact]
	public void Constructor_ForMacOS_ShouldInitializeAccordingly()
	{
		#pragma warning disable CS0618
		Execute sut = new(new MockFileSystem(), SimulationMode.MacOS);
		#pragma warning restore CS0618

		sut.IsLinux.Should().BeFalse();
		sut.IsMac.Should().BeTrue();
		sut.IsNetFramework.Should().BeFalse();
		sut.IsWindows.Should().BeFalse();
		sut.StringComparisonMode.Should().Be(StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void Constructor_ForWindows_ShouldInitializeAccordingly()
	{
		#pragma warning disable CS0618
		Execute sut = new(new MockFileSystem(), SimulationMode.Windows);
		#pragma warning restore CS0618

		sut.IsLinux.Should().BeFalse();
		sut.IsMac.Should().BeFalse();
		sut.IsNetFramework.Should().BeFalse();
		sut.IsWindows.Should().BeTrue();
		sut.StringComparisonMode.Should().Be(StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void Constructor_UnsupportedSimulationMode_ShouldThrowNotSupportedException()
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = new Execute(new MockFileSystem(), (SimulationMode)42);
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should()
			.Contain(nameof(SimulationMode.Linux)).And
			.Contain(nameof(SimulationMode.MacOS)).And
			.Contain(nameof(SimulationMode.Windows));
	}
}
