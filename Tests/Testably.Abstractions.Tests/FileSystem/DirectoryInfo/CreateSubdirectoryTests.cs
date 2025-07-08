using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class CreateSubdirectoryTests
{
	[Theory]
	[AutoData]
	public void CreateSubdirectory_FileWithSameNameAlreadyExists_ShouldThrowIOException(
		string name)
	{
		FileSystem.File.WriteAllText(name, "");
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(".");

		Exception? exception = Record.Exception(() =>
		{
			sut.CreateSubdirectory(name);
		});

		exception.Should().BeException<IOException>(
			hResult: Test.RunsOnWindows ? -2147024713 : 17);
		FileSystem.Directory.Exists(name).Should().BeFalse();
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
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
		await That(result.Exists).IsTrue();
		FileSystem.Directory.Exists(result.FullName).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public async Task CreateSubdirectory_ShouldCreateDirectory(string path, string subdirectory)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Create();
		IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		await That(sut.Exists).IsTrue();
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
		await That(result.Exists).IsTrue();
		FileSystem.Directory.Exists(result.FullName).Should().BeTrue();
	}
}
