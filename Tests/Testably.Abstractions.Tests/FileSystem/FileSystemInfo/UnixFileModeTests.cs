#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

[FileSystemTests]
public partial class UnixFileModeTests
{
	[Theory]
	[AutoData]
	public async Task UnixFileMode_MissingFile_ShouldBeInitializedToMinusOne(
		string path)
	{
		UnixFileMode expected = (UnixFileMode)(-1);
		IFileInfo fileSystemInfo = FileSystem.FileInfo.New(path);

		await That(fileSystemInfo.UnixFileMode).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public void UnixFileMode_SetterShouldThrowPlatformNotSupportedException_OnWindows(
		string path, UnixFileMode unixFileMode)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IFileInfo fileSystemInfo = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			#pragma warning disable CA1416
			fileSystemInfo.UnixFileMode = unixFileMode;
			#pragma warning restore CA1416
		});

		exception.Should().BeException<PlatformNotSupportedException>(hResult: -2146233031);
	}

	[Theory]
	[AutoData]
	public async Task UnixFileMode_ShouldBeInitializedCorrectly(
		string path)
	{
		Skip.If(Test.RunsOnWindows);

		UnixFileMode expected = UnixFileMode.OtherRead |
								UnixFileMode.GroupRead |
								UnixFileMode.UserWrite |
								UnixFileMode.UserRead;
		FileSystem.File.WriteAllText(path, "some content");
		IFileInfo fileSystemInfo = FileSystem.FileInfo.New(path);

		await That(fileSystemInfo.UnixFileMode).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public async Task UnixFileMode_ShouldBeSettableOnLinux(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "some content");
		IFileInfo fileSystemInfo = FileSystem.FileInfo.New(path);

#pragma warning disable CA1416
		fileSystemInfo.UnixFileMode = unixFileMode;
#pragma warning restore CA1416

		await That(fileSystemInfo.UnixFileMode).IsEqualTo(unixFileMode);
	}
}
#endif
