using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class CreateSubdirectoryTests
{
	[Theory]
	[AutoData]
	public async Task CreateSubdirectory_FileWithSameNameAlreadyExists_ShouldThrowIOException(
		string name)
	{
		FileSystem.File.WriteAllText(name, "");
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(".");

		void Act()
		{
			sut.CreateSubdirectory(name);
		}

		await That(Act).Throws<IOException>().WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
		await That(FileSystem.Directory.Exists(name)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task CreateSubdirectory_MissingParent_ShouldCreateDirectory(
		string path, string subdirectory)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsFalse();
		IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		await That(sut.Exists).IsFalse();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsTrue();
		await That(result.Exists).IsTrue();
		await That(FileSystem.Directory.Exists(result.FullName)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task CreateSubdirectory_ShouldCreateDirectory(string path, string subdirectory)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Create();
		IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		await That(sut.Exists).IsTrue();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsTrue();
		await That(result.Exists).IsTrue();
		await That(FileSystem.Directory.Exists(result.FullName)).IsTrue();
	}
}
