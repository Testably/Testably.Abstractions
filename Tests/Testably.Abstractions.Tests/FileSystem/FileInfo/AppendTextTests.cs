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

		await That(FileSystem).HasFile(path).WithContent(appendText);
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

		await That(FileSystem).HasFile(path).WithContent(contents + appendText);
	}

#if NET8_0_OR_GREATER
	[Test]
	[AutoArguments]
	public async Task AppendText_ShouldRefreshExistsCache(string path)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		await That(fileInfo.Exists).IsFalse();

		fileInfo.AppendText().Dispose();

		await That(fileInfo.Exists).IsTrue();
	}
#else
	[Test]
	[AutoArguments]
	public async Task AppendText_ShouldNotRefreshExistsCache(string path)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		await That(fileInfo.Exists).IsFalse();

		fileInfo.AppendText().Dispose();

		await That(fileInfo.Exists).IsFalse();
		await That(FileSystem.File.Exists(path)).IsTrue();
	}
#endif
}
