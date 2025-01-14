using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class OpenTextTests
{
	[SkippableTheory]
	[AutoData]
	public void OpenText_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			using StreamReader stream = FileSystem.File.OpenText(path);
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

		using StreamReader stream = FileSystem.File.OpenText(path);

		string result = stream.ReadToEnd();
		result.Should().Be(contents);
	}
}
