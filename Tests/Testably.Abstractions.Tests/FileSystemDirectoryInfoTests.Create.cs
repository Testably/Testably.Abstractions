namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    public void Create_ShouldCreateDirectory(string path)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeFalse();

        sut.Create();

#if NETFRAMEWORK
        // The DirectoryInfo is not updated in .NET Framework!
        sut.Exists.Should().BeFalse();
#else
        sut.Exists.Should().BeTrue();
#endif
        FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
    }

    [Theory]
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

    [Theory]
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