using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
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

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
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