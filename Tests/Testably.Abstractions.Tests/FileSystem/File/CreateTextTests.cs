using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class CreateTextTests
{
	[SkippableTheory]
	[AutoData]
	public void CreateText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		using (StreamWriter stream = FileSystem.File.CreateText(path))
		{
			stream.Write(appendText);
		}

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(appendText);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateText_ShouldReplaceTextInExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);

		using (StreamWriter stream = FileSystem.File.CreateText(path))
		{
			stream.Write(appendText);
		}

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(appendText);
	}
}
