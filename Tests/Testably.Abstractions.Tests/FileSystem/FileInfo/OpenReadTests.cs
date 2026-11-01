using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class OpenReadTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task OpenRead_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			_ = sut.OpenRead();
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessage().Containing($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Test]
	[AutoArguments]
	public async Task OpenRead_ShouldResetCachedStateOnlySinceNet11(string path)
	{
		FileSystem.File.WriteAllText(path, "a");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Length).IsEqualTo(1);
		FileSystem.File.WriteAllText(path, "abc");

		sut.OpenRead().Dispose();

#if FEATURE_FILEINFO_OPEN_INVALIDATES
		await That(sut.Length).IsEqualTo(3);
#else
		await That(sut.Length).IsEqualTo(1);
#endif
	}

	[Test]
	[AutoArguments]
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
