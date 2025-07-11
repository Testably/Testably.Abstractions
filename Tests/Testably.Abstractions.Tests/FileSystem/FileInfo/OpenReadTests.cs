using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class OpenReadTests
{
	[Theory]
	[AutoData]
	public async Task OpenRead_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			_ = sut.OpenRead();
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task OpenRead_ShouldUseReadAccessAndReadShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.OpenRead();

		await That(FileTestHelper.CheckFileAccess(stream)).IsEqualTo(FileAccess.Read);
		await That(FileTestHelper.CheckFileShare(FileSystem, path)).IsEqualTo(
			Test.RunsOnWindows ? FileShare.Read : FileShare.ReadWrite);
	}
}
