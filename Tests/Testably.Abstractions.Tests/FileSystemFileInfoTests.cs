using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public abstract string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileInfoTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
    }

    #endregion

    [Fact]
    [FileSystemTests.FileInfo]
    public void Directory_ShouldReturnParentDirectory()
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.Initialize()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile());
        IFileSystem.IFileInfo? file = initialized[1] as IFileSystem.IFileInfo;

        file?.Directory.Should().NotBeNull();
        file!.Directory!.FullName.Should().Be(initialized[0].FullName);
    }

    [Fact]
    [FileSystemTests.FileInfo]
    public void DirectoryName_ShouldReturnNameOfParentDirectory()
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.Initialize()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile());
        IFileSystem.IFileInfo? file = initialized[1] as IFileSystem.IFileInfo;

        file?.Should().NotBeNull();
        file!.DirectoryName.Should().Be(initialized[0].FullName);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo]
    public void IsReadOnly_SetToFalse_ShouldRemoveReadOnlyAttribute(string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);
        fileInfo.Attributes = FileAttributes.ReadOnly | FileAttributes.Hidden;

        fileInfo.IsReadOnly = false;

        fileInfo.IsReadOnly.Should().BeFalse();
        if (Test.RunsOnWindows)
        {
            fileInfo.Attributes.Should().Be(FileAttributes.Hidden);
        }
        else
        {
            fileInfo.Attributes.Should().Be(FileAttributes.Normal);
        }
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo]
    public void IsReadOnly_SetToTrue_ShouldAddReadOnlyAttribute(string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        fileInfo.IsReadOnly = true;

        fileInfo.IsReadOnly.Should().BeTrue();
        fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo]
    public void IsReadOnly_ShouldChangeWhenSettingReadOnlyAttribute(string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        fileInfo.Attributes = FileAttributes.ReadOnly | FileAttributes.Encrypted;

        fileInfo.IsReadOnly.Should().BeTrue();
        fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo]
    public void IsReadOnly_ShouldInitializeToReadOnlyAttribute(string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        fileInfo.IsReadOnly.Should().BeFalse();
        fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly).Should().BeFalse();
    }
}