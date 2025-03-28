using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class CreateTextTests
{
	[Theory]
	[AutoData]
	public void CreateText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(appendText);
	}

#if NET8_0_OR_GREATER
	[Theory]
	[AutoData]
	public void CreateText_ShouldRefreshExistsCache(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.Exists.Should().BeFalse();

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		fileInfo.Exists.Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeTrue();
	}
#else
	[Theory]
	[AutoData]
	public void CreateText_ShouldNotRefreshExistsCache(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.Exists.Should().BeFalse();

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		fileInfo.Exists.Should().BeFalse();
		FileSystem.File.Exists(path).Should().BeTrue();
	}
#endif

	[Theory]
	[AutoData]
	public void CreateText_ShouldReplaceTextInExistingFile(
		string path, string contents, string appendText)
	{
		FileSystem.File.WriteAllText(path, contents);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(appendText);
	}
}
