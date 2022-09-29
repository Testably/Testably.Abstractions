using System.IO;

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

    [Fact]
    [Trait(nameof(FileSystem), nameof(DirectoryInfo))]
    [Trait(nameof(DirectoryInfo), nameof(IFileSystem.IDirectoryInfoFactory.New))]
    public void Create_Empty_ShouldThrowArgumentException()
    {
        Exception? exception =
            Record.Exception(() => FileSystem.DirectoryInfo.New(string.Empty));

#if NETFRAMEWORK
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should().Be("The path is not of a legal form.");
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.ParamName.Should().Be("path");
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should()
           .Be("The path is empty. (Parameter 'path')");
#endif
    }
}