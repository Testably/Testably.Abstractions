using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class CreateTextTests
{
	[Theory]
	[AutoData]
	public async Task CreateText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		using (StreamWriter stream = FileSystem.File.CreateText(path))
		{
			stream.Write(appendText);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(appendText);
	}

	[Theory]
	[AutoData]
	public async Task CreateText_ShouldReplaceTextInExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);

		using (StreamWriter stream = FileSystem.File.CreateText(path))
		{
			stream.Write(appendText);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(appendText);
	}
}
