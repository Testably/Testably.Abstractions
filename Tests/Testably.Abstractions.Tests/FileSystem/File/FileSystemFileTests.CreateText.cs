using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.CreateText))]
	public void CreateText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		using (StreamWriter stream = FileSystem.File.CreateText(path))
		{
			stream.Write(appendText);
		}

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(appendText);
		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.CreateText))]
	public void CreateText_ShouldReplaceTextInExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);

		using (StreamWriter stream = FileSystem.File.CreateText(path))
		{
			stream.Write(appendText);
		}

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(appendText);
	}
}