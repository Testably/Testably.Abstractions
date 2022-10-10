using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.SetAttributes))]
	public void SetAttributes_GetAttributesShouldReturnAttributes(
		string path, FileAttributes attributes)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		result.Should().Be(attributes);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.SetAttributes))]
	public void SetAttributes_MissingFile_GetAttributesShouldReturnAttributes(
		string path, FileAttributes attributes)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetAttributes(path, attributes);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}
}