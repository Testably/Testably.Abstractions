using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class CreateSubdirectoryTests
{
	[SkippableTheory]
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
		FileSystem.Should().NotHaveDirectory(name);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSubdirectory_MissingParent_ShouldCreateDirectory(
		string path, string subdirectory)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().NotExist();
		IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		sut.Should().NotExist();
		FileSystem.Should().HaveDirectory(sut.FullName);
		result.Should().Exist();
		FileSystem.Should().HaveDirectory(result.FullName);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSubdirectory_ShouldCreateDirectory(string path, string subdirectory)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Create();
		IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		sut.Should().Exist();
		FileSystem.Should().HaveDirectory(sut.FullName);
		result.Should().Exist();
		FileSystem.Should().HaveDirectory(result.FullName);
	}
}
