using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
#endif

namespace Testably.Abstractions.Testing.Tests;

public class MockFileSystemInitializationTests
{
	[Fact]
	public void MockFileSystem_WhenSimulatingLinux_ShouldBeLinux()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(OSPlatform.Linux));

		sut.Execute.IsLinux.Should().BeTrue();
		sut.Execute.IsMac.Should().BeFalse();
		sut.Execute.IsWindows.Should().BeFalse();
		sut.Execute.IsNetFramework.Should().BeFalse();
	}

	[Fact]
	public void MockFileSystem_WhenSimulatingOSX_ShouldBeMac()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(OSPlatform.OSX));

		sut.Execute.IsLinux.Should().BeFalse();
		sut.Execute.IsMac.Should().BeTrue();
		sut.Execute.IsWindows.Should().BeFalse();
		sut.Execute.IsNetFramework.Should().BeFalse();
	}

	[Fact]
	public void MockFileSystem_WhenSimulatingWindows_ShouldBeWindows()
	{
		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(OSPlatform.Windows));

		sut.Execute.IsLinux.Should().BeFalse();
		sut.Execute.IsMac.Should().BeFalse();
		sut.Execute.IsWindows.Should().BeTrue();
		sut.Execute.IsNetFramework.Should().BeFalse();
	}

#if !NET48
	[Fact]
	public void SimulatingOperatingSystem_FreeBSD_ShouldThrowNotSupportedException()
	{
		MockFileSystem.Initialization sut = new();

		Exception? exception = Record.Exception(() =>
		{
			sut.SimulatingOperatingSystem(OSPlatform.FreeBSD);
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should()
			.Contain("Linux").And
			.Contain("Windows").And
			.Contain("OSX");
	}
#endif

	[Theory]
	[MemberData(nameof(ValidOperatingSystems))]
	public void SimulatingOperatingSystem_ValidOSPlatform_ShouldSetOperatingSystem(
		OSPlatform osPlatform)
	{
		MockFileSystem.Initialization sut = new();

		MockFileSystem.Initialization result = sut.SimulatingOperatingSystem(osPlatform);

		result.OperatingSystem.Should().Be(osPlatform);
		sut.OperatingSystem.Should().Be(osPlatform);
	}

	#region Helpers

	public static TheoryData<OSPlatform> ValidOperatingSystems()
		=> new(OSPlatform.Linux, OSPlatform.OSX, OSPlatform.Windows);

	#endregion
}
