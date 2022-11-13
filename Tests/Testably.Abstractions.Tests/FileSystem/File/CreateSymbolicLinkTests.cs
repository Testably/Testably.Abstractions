#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateSymbolicLinkTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);

		FileSystem.File.CreateSymbolicLink(path, pathToTarget);

		FileSystem.File.GetAttributes(path)
			.HasFlag(FileAttributes.ReparsePoint)
			.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_SourceFileAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(path, pathToTarget);
		});

		exception.Should().BeException<IOException>($"'{path}'",
			hResult: Test.RunsOnWindows ? -2147024713 : 17);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSymbolicLink_TargetFileMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(path, pathToTarget);
		});

		exception.Should().BeNull();
	}
}
#endif