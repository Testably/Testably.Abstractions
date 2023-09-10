using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateTextTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateText_MissingFile_ShouldCreateFile(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(appendText);
	}

#if NET8_0_OR_GREATER
	[SkippableTheory]
	[AutoData]
	public void CreateText_ShouldRefreshExistsCache(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.Should().NotExist();

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		fileInfo.Should().Exist();
		FileSystem.Should().HaveFile(path);
	}
#else
	[SkippableTheory]
	[AutoData]
	public void CreateText_ShouldNotRefreshExistsCache(
		string path, string appendText)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.Should().NotExist();

		using (StreamWriter stream = fileInfo.CreateText())
		{
			stream.Write(appendText);
		}

		fileInfo.Should().NotExist();
		FileSystem.Should().HaveFile(path);
	}
#endif

	[SkippableTheory]
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

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(appendText);
	}
}
