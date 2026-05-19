using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class CreateTextTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task CreateText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		using (StreamWriter stream = FileSystem.File.CreateText(path))
		{
			stream.Write(appendText);
		}

		await That(FileSystem).HasFile(path).WithContent(appendText);
	}

	[Test]
	[AutoArguments]
	public async Task CreateText_ShouldReplaceTextInExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);

		using (StreamWriter stream = FileSystem.File.CreateText(path))
		{
			stream.Write(appendText);
		}

		await That(FileSystem).HasFile(path).WithContent(appendText);
	}
}
