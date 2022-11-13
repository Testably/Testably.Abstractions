#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateAsSymbolicLinkTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_ShouldCreateAsSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(pathToTarget);

		FileSystem.DirectoryInfo.New(path).Attributes
			.HasFlag(FileAttributes.ReparsePoint)
			.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_SourceDirectoryAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeException<IOException>($"'{path}'",
			hResult: Test.RunsOnWindows ? -2147024713 : 17);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_TargetDirectoryMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithIllegalCharactersInTarget_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink("bar_?_");
		});

		exception.Should().BeException<IOException>(hResult: -2147024713);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateAsSymbolicLink_WithIllegalTarget_ShouldNotThrowException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(" ");
		});

		exception.Should().BeNull();
	}
}
#endif