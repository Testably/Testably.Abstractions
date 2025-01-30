using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class AppendTextTests
{
	[Theory]
	[AutoData]
	public void AppendText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.AppendText())
		{
			stream.Write(appendText);
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(appendText);
	}

	[Theory]
	[AutoData]
	public void AppendText_ShouldAddTextToExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.AppendText())
		{
			stream.Write(appendText);
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents + appendText);
	}
}
