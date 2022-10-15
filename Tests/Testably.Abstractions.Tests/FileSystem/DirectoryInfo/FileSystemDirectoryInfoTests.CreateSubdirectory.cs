namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateSubdirectory_MissingParent_ShouldCreateDirectory(
		string path, string subdirectory)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeFalse();
		IFileSystem.IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		sut.Exists.Should().BeFalse();
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
		result.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(result.FullName).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSubdirectory_ShouldCreateDirectory(string path, string subdirectory)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Create();
		IFileSystem.IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		sut.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
		result.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(result.FullName).Should().BeTrue();
	}
}