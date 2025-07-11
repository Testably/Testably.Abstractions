#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class UnixFileModeTests
{
	[Theory]
	[AutoData]
	public async Task GetUnixFileMode_ShouldBeInitializedCorrectly(
		string path)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "");
		UnixFileMode expected = UnixFileMode.OtherRead |
								UnixFileMode.GroupRead |
								UnixFileMode.UserWrite |
								UnixFileMode.UserRead;

#pragma warning disable CA1416
		UnixFileMode result = FileSystem.File.GetUnixFileMode(path);
#pragma warning restore CA1416

		await That(result).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public async Task GetUnixFileMode_ShouldThrowPlatformNotSupportedException_OnWindows(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			#pragma warning disable CA1416
			FileSystem.File.GetUnixFileMode(path);
			#pragma warning restore CA1416
		}

		await That(Act).Throws<PlatformNotSupportedException>().WithHResult(-2146233031);
	}

	[Theory]
	[AutoData]
	public async Task SetUnixFileMode_MissingFile_ShouldThrowFileNotFoundException(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		void Act()
		{
			#pragma warning disable CA1416
			FileSystem.File.SetUnixFileMode(path, unixFileMode);
			#pragma warning restore CA1416
		}

		await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task SetUnixFileMode_ShouldBeSettableOnLinux(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "some content");

#pragma warning disable CA1416
		FileSystem.File.SetUnixFileMode(path, unixFileMode);

		UnixFileMode result = FileSystem.File.GetUnixFileMode(path);
#pragma warning restore CA1416
		await That(result).IsEqualTo(unixFileMode);
	}

	[Theory]
	[AutoData]
	public async Task SetUnixFileMode_ShouldThrowPlatformNotSupportedException_OnWindows(
		string path, UnixFileMode mode)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			#pragma warning disable CA1416
			FileSystem.File.SetUnixFileMode(path, mode);
			#pragma warning restore CA1416
		}

		await That(Act).Throws<PlatformNotSupportedException>().WithHResult(-2146233031);
	}
}
#endif
