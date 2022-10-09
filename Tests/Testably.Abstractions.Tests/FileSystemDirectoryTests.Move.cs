using System.IO;
#if !NETFRAMEWORK
#endif

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.Move))]
    public void Move_ShouldMoveDirectoryWithContent(string source, string destination)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.InitializeIn(source)
               .WithAFile()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile()
                   .WithASubdirectory());

        FileSystem.Directory.Move(source, destination);

        FileSystem.Directory.Exists(source).Should().BeFalse();
        FileSystem.Directory.Exists(destination).Should().BeTrue();
        FileSystem.Directory.GetFiles(destination, initialized[0].Name)
           .Should().ContainSingle();
        FileSystem.Directory.GetDirectories(destination, initialized[1].Name)
           .Should().ContainSingle();
        FileSystem.Directory.GetFiles(destination, initialized[2].Name,
                SearchOption.AllDirectories)
           .Should().ContainSingle();
        FileSystem.Directory.GetDirectories(destination, initialized[3].Name,
                SearchOption.AllDirectories)
           .Should().ContainSingle();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.Move))]
    public void Move_WithLockedFile_ShouldNotMoveDirectoryAtAll(
        string source, string destination)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.InitializeIn(source)
               .WithAFile()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile()
                   .WithASubdirectory());
        using FileSystemStream stream = FileSystem.File.Open(initialized[2].FullName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.Move(source, destination);
        });

        if (Test.RunsOnWindows)
        {
            exception.Should().BeOfType<IOException>();
            FileSystem.Directory.Exists(source).Should().BeTrue();
            FileSystem.Directory.Exists(destination).Should().BeFalse();
            IFileSystem.IDirectoryInfo sourceDirectory =
                FileSystem.DirectoryInfo.New(source);
            sourceDirectory.GetFiles(initialized[0].Name)
               .Should().ContainSingle();
            sourceDirectory.GetDirectories(initialized[1].Name)
               .Should().ContainSingle();
            sourceDirectory.GetFiles(initialized[2].Name, SearchOption.AllDirectories)
               .Should().ContainSingle();
            sourceDirectory
               .GetDirectories(initialized[3].Name, SearchOption.AllDirectories)
               .Should().ContainSingle();
        }
        else
        {
            exception.Should().BeNull();
            FileSystem.Directory.Exists(source).Should().BeFalse();
            FileSystem.Directory.Exists(destination).Should().BeTrue();
            IFileSystem.IDirectoryInfo destinationDirectory =
                FileSystem.DirectoryInfo.New(destination);
            destinationDirectory.GetFiles(initialized[0].Name)
               .Should().ContainSingle();
            destinationDirectory.GetDirectories(initialized[1].Name)
               .Should().ContainSingle();
            destinationDirectory.GetFiles(initialized[2].Name, SearchOption.AllDirectories)
               .Should().ContainSingle();
            destinationDirectory
               .GetDirectories(initialized[3].Name, SearchOption.AllDirectories)
               .Should().ContainSingle();
        }
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.Move))]
    public void Move_WithReadOnlyFile_ShouldMoveDirectoryWithContent(
        string source, string destination)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.InitializeIn(source)
               .WithAFile()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile()
                   .WithASubdirectory());
        initialized[2].Attributes = FileAttributes.ReadOnly;

        FileSystem.Directory.Move(source, destination);

        FileSystem.Directory.Exists(source).Should().BeFalse();
        FileSystem.Directory.Exists(destination).Should().BeTrue();
        IFileSystem.IDirectoryInfo destinationDirectory =
            FileSystem.DirectoryInfo.New(destination);
        destinationDirectory.GetFiles(initialized[0].Name)
           .Should().ContainSingle();
        destinationDirectory.GetDirectories(initialized[1].Name)
           .Should().ContainSingle();
        destinationDirectory.GetFiles(initialized[2].Name, SearchOption.AllDirectories)
           .Should().ContainSingle().Which.Attributes.Should()
           .HaveFlag(FileAttributes.ReadOnly);
        destinationDirectory
           .GetDirectories(initialized[3].Name, SearchOption.AllDirectories)
           .Should().ContainSingle();
    }
}