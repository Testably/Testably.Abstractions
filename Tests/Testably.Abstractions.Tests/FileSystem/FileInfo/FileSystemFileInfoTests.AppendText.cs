using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void AppendText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.AppendText())
		{
			stream.Write(appendText);
		}

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(appendText);
		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void AppendText_ShouldAddTextToExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.AppendText())
		{
			stream.Write(appendText);
		}

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(contents + appendText);
	}
}