using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(appendText);
		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateText_ShouldReplaceTextInExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(appendText);
	}
}