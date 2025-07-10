using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class OpenTextTests
{
	[Theory]
	[AutoData]
	public async Task OpenText_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		void Act()
		{
			using StreamReader stream = FileSystem.File.OpenText(path);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task OpenText_ShouldReturnFileContent(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using StreamReader stream = FileSystem.File.OpenText(path);

		string result = stream.ReadToEnd();
		await That(result).IsEqualTo(contents);
	}
}
