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
	public void CreateSymbolicLink_SourceDirectoryMissing_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string sourceFile, string pathToTarget)
	{
		string sourcePath = FileSystem.Path.Combine(missingDirectory, sourceFile);
		FileSystem.File.WriteAllText(pathToTarget, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.CreateSymbolicLink(sourcePath, pathToTarget);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
		         .Which.HResult.Should().Be(-2147024893);
		exception.Should().BeOfType<DirectoryNotFoundException>()
		         .Which.Message.Should().Contain($"{sourcePath}'");
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

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
			         .Which.HResult.Should().Be(-2147024713);
		}
		else
		{
			exception.Should().BeOfType<IOException>()
			         .Which.HResult.Should().Be(17);
		}

		exception.Should().BeOfType<IOException>()
		         .Which.Message.Should().Contain($"'{path}'");
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