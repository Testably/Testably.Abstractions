using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class AppendTextTests
{
	[SkippableTheory]
	[AutoData]
	public void AppendText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		using (StreamWriter stream = FileSystem.File.AppendText(path))
		{
			stream.Write(appendText);
		}

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(appendText);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendText_ShouldAddTextToExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);

		using (StreamWriter stream = FileSystem.File.AppendText(path))
		{
			stream.Write(appendText);
		}

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(contents + appendText);
	}
}
