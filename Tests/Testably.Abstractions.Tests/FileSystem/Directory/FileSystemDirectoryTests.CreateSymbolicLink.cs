#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	
	public void CreateSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);

		FileSystem.DirectoryInfo.New(path).Attributes
		   .HasFlag(FileAttributes.ReparsePoint)
		   .Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	
	public void CreateSymbolicLink_SourceDirectoryAlreadyExists_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);
		});

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should().Contain($"'{path}'");
	}
}
#endif