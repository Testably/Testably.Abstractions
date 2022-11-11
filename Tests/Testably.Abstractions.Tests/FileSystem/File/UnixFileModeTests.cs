#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class UnixFileModeTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void GetUnixFileMode_MissingFile_ShouldBeInitializedToMinusOneOnWindows(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		UnixFileMode expected = (UnixFileMode)(-1);
		UnixFileMode result = FileSystem.File.GetUnixFileMode(path);

		result.Should().Be(expected);
	}

	[SkippableTheory]
	[AutoData]
	public void GetUnixFileMode_ShouldThrowNotSupportedExceptionOnWindows(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.GetUnixFileMode(path);
		});

		exception.Should().BeOfType<PlatformNotSupportedException>()
		   .Which.HResult.Should().Be(-2146233031);
	}

	[SkippableTheory]
	[AutoData]
	public void SetUnixFileMode_ShouldBeSettableOnLinux(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "some content");

		FileSystem.File.SetUnixFileMode(path, unixFileMode);

		UnixFileMode result = FileSystem.File.GetUnixFileMode(path);
		result.Should().Be(unixFileMode);
	}

	[SkippableTheory]
	[AutoData]
	public void SetUnixFileMode_MissingFile_ShouldThrowFileNotFoundException(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetUnixFileMode(path, unixFileMode);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public void SetUnixFileMode_ShouldThrowNotSupportedExceptionOnWindows(
		string path, UnixFileMode mode)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetUnixFileMode(path, mode);
		});

		exception.Should().BeOfType<PlatformNotSupportedException>()
		   .Which.HResult.Should().Be(-2146233031);
	}
}
#endif