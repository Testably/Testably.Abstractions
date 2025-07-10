using NSubstitute.ExceptionExtensions;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class OpenTextTests
{
	[Theory]
	[AutoData]
	public async Task OpenText_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		void Act()
		{
			using StreamReader stream = fileInfo.OpenText();
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
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using StreamReader stream = fileInfo.OpenText();

		string result = stream.ReadToEnd();
		await That(result).IsEqualTo(contents);
	}
}
