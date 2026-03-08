using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class AppendTextTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task AppendText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.AppendText())
		{
			stream.Write(appendText);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(appendText);
	}

	[Test]
	[AutoArguments]
	public async Task AppendText_ShouldAddTextToExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.AppendText())
		{
			stream.Write(appendText);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(contents + appendText);
	}
}
