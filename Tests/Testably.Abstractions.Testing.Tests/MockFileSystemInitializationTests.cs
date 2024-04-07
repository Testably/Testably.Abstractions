using System.IO;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#if NET6_0_OR_GREATER
#endif

namespace Testably.Abstractions.Testing.Tests;

public class MockFileSystemInitializationTests
{
	[Fact]
	public void MockFileSystem_WhenSimulatingLinux_ShouldBeLinux()
	{
		Skip.IfNot(Test.RunsOnLinux,
			"TODO: Enable again, once the Path implementation is sufficiently complete!");

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
		Skip.IfNot(Test.RunsOnMac,
			"TODO: Enable again, once the Path implementation is sufficiently complete!");

		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(OSPlatform.OSX));

		sut.Execute.IsLinux.Should().BeFalse();
		sut.Execute.IsMac.Should().BeTrue();
		sut.Execute.IsWindows.Should().BeFalse();
		sut.Execute.IsNetFramework.Should().BeFalse();
	}

	[SkippableFact]
	public void MockFileSystem_WhenSimulatingWindows_ShouldBeWindows()
	{
		Skip.IfNot(Test.RunsOnWindows,
			"TODO: Enable again, once the Path implementation is sufficiently complete!");

		MockFileSystem sut = new(o => o
			.SimulatingOperatingSystem(OSPlatform.Windows));

		sut.Execute.IsLinux.Should().BeFalse();
		sut.Execute.IsMac.Should().BeFalse();
		sut.Execute.IsWindows.Should().BeTrue();
		sut.Execute.IsNetFramework.Should().BeFalse();
	}

	[Fact]
	public void MockFileSystem_WithCurrentDirectory_ShouldInitializeCurrentDirectory()
	{
		string expected = Directory.GetCurrentDirectory();
		MockFileSystem sut = new(o => o.UseCurrentDirectory());

		string result = sut.Directory.GetCurrentDirectory();

		result.Should().Be(expected);
	}

	[Theory]
	[AutoData]
	public void MockFileSystem_WithExplicitCurrentDirectory_ShouldInitializeCurrentDirectory(
		string path)
	{
		string expected = Test.RunsOnWindows ? $"C:\\{path}" : $"/{path}";
		MockFileSystem sut = new(o => o.UseCurrentDirectory(path));

		string result = sut.Directory.GetCurrentDirectory();

		result.Should().Be(expected);
	}

	[Fact]
	public void MockFileSystem_WithoutCurrentDirectory_ShouldUseDefaultDriveAsCurrentDirectory()
	{
		string expected = Test.RunsOnWindows ? "C:\\" : "/";
		MockFileSystem sut = new();

		string result = sut.Directory.GetCurrentDirectory();

		result.Should().Be(expected);
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

	[Fact]
	public void UseCurrentDirectory_Empty_ShouldUseCurrentDirectory()
	{
		string expected = Directory.GetCurrentDirectory();
		MockFileSystem.Initialization sut = new();

		MockFileSystem.Initialization result = sut.UseCurrentDirectory();

		result.CurrentDirectory.Should().Be(expected);
		sut.CurrentDirectory.Should().Be(expected);
	}

	[Theory]
	[AutoData]
	public void UseCurrentDirectory_WithPath_ShouldUsePathCurrentDirectory(string path)
	{
		MockFileSystem.Initialization sut = new();

		MockFileSystem.Initialization result = sut.UseCurrentDirectory(path);

		result.CurrentDirectory.Should().Be(path);
		sut.CurrentDirectory.Should().Be(path);
	}

	#region Helpers

	public static TheoryData<OSPlatform> ValidOperatingSystems()
		=> new(OSPlatform.Linux, OSPlatform.OSX, OSPlatform.Windows);

	#endregion
}
