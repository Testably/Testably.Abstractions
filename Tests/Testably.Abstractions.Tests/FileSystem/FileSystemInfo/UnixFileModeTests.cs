#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class UnixFileModeTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void UnixFileMode_MissingFile_ShouldBeInitializedToMinusOne(
		string path)
	{
		UnixFileMode expected = (UnixFileMode)(-1);
		IFileInfo fileSystemInfo = FileSystem.FileInfo.New(path);

		fileSystemInfo.UnixFileMode.Should().Be(expected);
	}

	[SkippableTheory]
	[AutoData]
	public void UnixFileMode_ShouldBeInitializedToMinusOne(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		UnixFileMode expected = (UnixFileMode)(-1);
		FileSystem.File.WriteAllText(path, "some content");
		IFileInfo fileSystemInfo = FileSystem.FileInfo.New(path);

		fileSystemInfo.UnixFileMode.Should().Be(expected);
	}

	[SkippableTheory]
	[AutoData]
	public void UnixFileMode_ShouldBeSettableOnLinux(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "some content");
		IFileInfo fileSystemInfo = FileSystem.FileInfo.New(path);

		#pragma warning disable CA1416
		fileSystemInfo.UnixFileMode = unixFileMode;
		#pragma warning restore CA1416

		fileSystemInfo.UnixFileMode.Should().Be(unixFileMode);
	}

	[SkippableTheory]
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
}
#endif
