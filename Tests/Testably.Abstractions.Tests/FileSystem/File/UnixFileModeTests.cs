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
	public void GetUnixFileMode_ShouldThrowPlatformNotSupportedException_OnWindows(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			#pragma warning disable CA1416
			FileSystem.File.GetUnixFileMode(path);
			#pragma warning restore CA1416
		});

		exception.Should().BeException<PlatformNotSupportedException>(hResult: -2146233031);
	}

	[SkippableTheory]
	[AutoData]
	public void SetUnixFileMode_MissingFile_ShouldThrowFileNotFoundException(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			#pragma warning disable CA1416
			FileSystem.File.SetUnixFileMode(path, unixFileMode);
			#pragma warning restore CA1416
		});

		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public void SetUnixFileMode_ShouldBeSettableOnLinux(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "some content");

		#pragma warning disable CA1416
		FileSystem.File.SetUnixFileMode(path, unixFileMode);

		UnixFileMode result = FileSystem.File.GetUnixFileMode(path);
		#pragma warning restore CA1416
		result.Should().Be(unixFileMode);
	}

	[SkippableTheory]
	[AutoData]
	public void SetUnixFileMode_ShouldThrowPlatformNotSupportedException_OnWindows(
		string path, UnixFileMode mode)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			#pragma warning disable CA1416
			FileSystem.File.SetUnixFileMode(path, mode);
			#pragma warning restore CA1416
		});

		exception.Should().BeException<PlatformNotSupportedException>(hResult: -2146233031);
	}
}
#endif
