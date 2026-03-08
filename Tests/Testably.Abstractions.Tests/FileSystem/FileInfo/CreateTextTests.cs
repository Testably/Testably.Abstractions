using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class CreateTextTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task CreateText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(appendText);
	}

#if NET8_0_OR_GREATER
	[Test]
	[AutoArguments]
	public async Task CreateText_ShouldRefreshExistsCache(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		await That(fileInfo.Exists).IsFalse();

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		await That(fileInfo.Exists).IsTrue();
		await That(FileSystem.File.Exists(path)).IsTrue();
	}
#else
	[Test]
	[AutoArguments]
	public async Task CreateText_ShouldNotRefreshExistsCache(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		await That(fileInfo.Exists).IsFalse();

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		await That(fileInfo.Exists).IsFalse();
		await That(FileSystem.File.Exists(path)).IsTrue();
	}
#endif

	[Test]
	[AutoArguments]
	public async Task CreateText_ShouldReplaceTextInExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(appendText);
	}
}
