using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class OpenTextTests
{
	[SkippableTheory]
	[AutoData]
	public void OpenText_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			using StreamReader stream = fileInfo.OpenText();
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public void OpenText_ShouldReturnFileContent(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using StreamReader stream = fileInfo.OpenText();

		string result = stream.ReadToEnd();
		result.Should().Be(contents);
	}
}
