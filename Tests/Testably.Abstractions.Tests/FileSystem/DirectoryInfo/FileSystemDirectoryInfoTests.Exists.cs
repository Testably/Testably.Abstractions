namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Exists_ArbitraryPath_ShouldBeFalse(string path)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Exists.Should().BeFalse();
		FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();
		FileSystem.Directory.Delete(path);

		sut.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Exists.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_NotExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeFalse();
		FileSystem.Directory.CreateDirectory(path);

		sut.Exists.Should().BeFalse();
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
	}
}