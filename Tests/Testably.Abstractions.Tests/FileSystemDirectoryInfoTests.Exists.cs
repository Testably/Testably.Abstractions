namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Exists))]
    public void Exists_ArbitraryPath_ShouldBeFalse(string path)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

        sut.Exists.Should().BeFalse();
        FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Exists))]
    public void Exists_ExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
    {
        FileSystem.Directory.CreateDirectory(path);
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeTrue();
        FileSystem.Directory.Delete(path);

        sut.Exists.Should().BeTrue();
        FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Exists))]
    public void Exists_NotExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeFalse();
        FileSystem.Directory.CreateDirectory(path);

        sut.Exists.Should().BeFalse();
        FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
    }
}