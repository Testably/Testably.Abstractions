using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.AppendText))]
	public void AppendText_ShouldAddTextToExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);
		IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.AppendText())
		{
			stream.Write(appendText);
		}

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(contents + appendText);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.AppendText))]
	public void AppendText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.AppendText())
		{
			stream.Write(appendText);
		}

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(appendText);
		FileSystem.File.Exists(path).Should().BeTrue();
	}
}