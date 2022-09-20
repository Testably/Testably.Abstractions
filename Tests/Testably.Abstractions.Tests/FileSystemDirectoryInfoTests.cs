using System.IO;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemDirectoryInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }
    public string BasePath { get; }

    protected FileSystemDirectoryInfoTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem,
        string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }

    #endregion

    [Theory]
    [AutoData]
    public void Create_ShouldCreateDirectory(string path)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeFalse();

        sut.Create();

        sut.Exists.Should().BeTrue();
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

    [Theory]
    [AutoData]
    public void Delete_MissingDirectory_ShouldDeleteDirectory(string path)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeFalse();

        Exception? exception = Record.Exception(() =>
        {
            sut.Delete();
        });

        exception.Should().BeOfType<DirectoryNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find a part of the path '{sut.FullName}'.");
    }

    [Theory]
    [AutoData]
    public void Delete_Recursive_WithSubdirectory_ShouldDeleteDirectoryWithContent(
        string path, string subdirectory)
    {
        string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
        FileSystem.Directory.CreateDirectory(subdirectoryPath);
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeTrue();

        sut.Delete(true);

        sut.Exists.Should().BeFalse();
        FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
        FileSystem.Directory.Exists(subdirectoryPath).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void Delete_ShouldDeleteDirectory(string path)
    {
        FileSystem.Directory.CreateDirectory(path);
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeTrue();

        sut.Delete();

        sut.Exists.Should().BeFalse();
        FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void Delete_WithSubdirectory_ShouldNotDeleteDirectory(
        string path, string subdirectory)
    {
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subdirectory));
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeTrue();

        Exception? exception = Record.Exception(() =>
        {
            sut.Delete();
        });

        exception.Should().BeOfType<IOException>()
           .Which.Message.Should()
           .Be($"The directory is not empty. : '{sut.FullName}'");
        sut.Exists.Should().BeTrue();
        FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void Exists_ArbitraryPath_ShouldBeFalse(string path)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

        sut.Exists.Should().BeFalse();
        FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void Exists_ShouldOnlyUpdateOnInitialization(string path)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
        sut.Exists.Should().BeFalse();
        FileSystem.Directory.CreateDirectory(path);

        sut.Exists.Should().BeFalse();
        FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void Exists_ShouldOnlyUpdateOnInitialization2(string path)
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
    public void Parent_ArbitraryPaths_ShouldNotBeNull(string path1, string path2,
                                                      string path3)
    {
        string path = FileSystem.Path.Combine(path1, path2, path3);

        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

        sut.Parent.Should().NotBeNull();
        sut.Parent!.Exists.Should().BeFalse();
        sut.Parent.Parent.Should().NotBeNull();
        sut.Parent.Parent!.Exists.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void Root_ShouldExist(string path)
    {
        string expectedRoot = string.Empty.PrefixRoot();
        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

        result.Root.Exists.Should().BeTrue();
        result.Root.FullName.Should().Be(expectedRoot);
    }
}