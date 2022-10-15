using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.GetAttributes))]
	public void GetAttributes_MissingFile_GetAttributesShouldReturnAttributes(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.GetAttributes(path);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}
}