using System.IO;
#if !NETFRAMEWORK
using System.Runtime.InteropServices;
#endif

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(Directory))]
    [Trait(nameof(Directory), nameof(Directory.Delete))]
    public void Delete_FullPath_ShouldDeleteDirectory(string directoryName)
    {
        IFileSystem.IDirectoryInfo result =
            FileSystem.Directory.CreateDirectory(directoryName);

        FileSystem.Directory.Delete(result.FullName);

        FileSystem.Directory.Exists(directoryName).Should().BeFalse();
        result.Exists.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(Directory))]
    [Trait(nameof(Directory), nameof(Directory.Delete))]
    public void Delete_MissingDirectory_ShouldDeleteDirectory(string directoryName)
    {
        string expectedPath = Path.Combine(BasePath, directoryName);
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.Delete(directoryName);
        });

        exception.Should().BeOfType<DirectoryNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find a part of the path '{expectedPath}'.");
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(Directory))]
    [Trait(nameof(Directory), nameof(Directory.Delete))]
    public void Delete_Recursive_MissingDirectory_ShouldDeleteDirectory(
        string directoryName)
    {
        string expectedPath = Path.Combine(BasePath, directoryName);
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.Delete(directoryName, true);
        });

        exception.Should().BeOfType<DirectoryNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find a part of the path '{expectedPath}'.");
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(Directory))]
    [Trait(nameof(Directory), nameof(Directory.Delete))]
    public void
        Delete_Recursive_WithSimilarNamedFile_ShouldOnlyDeleteDirectoryAndItsContents(
            string subdirectory)
    {
        string fileName = $"{subdirectory}.txt";
        FileSystem.Initialize()
           .WithSubdirectory(subdirectory).Initialized(s => s
               .WithAFile()
               .WithASubdirectory())
           .WithFile(fileName);

        FileSystem.Directory.Delete(subdirectory, true);

        FileSystem.Directory.Exists(subdirectory).Should().BeFalse();
        FileSystem.File.Exists(fileName).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(Directory))]
    [Trait(nameof(Directory), nameof(Directory.Delete))]
    public void Delete_Recursive_WithSubdirectory_ShouldDeleteDirectoryWithContent(
        string path, string subdirectory)
    {
        string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
        FileSystem.Directory.CreateDirectory(subdirectoryPath);
        FileSystem.Directory.Exists(path).Should().BeTrue();

        FileSystem.Directory.Delete(path, true);

        FileSystem.Directory.Exists(path).Should().BeFalse();
        FileSystem.Directory.Exists(subdirectoryPath).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(Directory))]
    [Trait(nameof(Directory), nameof(Directory.Delete))]
    public void Delete_ShouldDeleteDirectory(string directoryName)
    {
        IFileSystem.IDirectoryInfo result =
            FileSystem.Directory.CreateDirectory(directoryName);

        FileSystem.Directory.Delete(directoryName);

        bool exists = FileSystem.Directory.Exists(directoryName);

        exists.Should().BeFalse();
        result.Exists.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(Directory))]
    [Trait(nameof(Directory), nameof(Directory.Delete))]
    public void Delete_WithSimilarNamedFile_ShouldOnlyDeleteDirectory(
        string subdirectory)
    {
        string fileName = $"{subdirectory}.txt";
        FileSystem.Initialize()
           .WithSubdirectory(subdirectory)
           .WithFile(fileName);

        FileSystem.Directory.Delete(subdirectory);

        FileSystem.Directory.Exists(subdirectory).Should().BeFalse();
        FileSystem.File.Exists(fileName).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(Directory))]
    [Trait(nameof(Directory), nameof(Directory.Delete))]
    public void Delete_WithSubdirectory_ShouldNotDeleteDirectory(
        string path, string subdirectory)
    {
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subdirectory));
        FileSystem.Directory.Exists(path).Should().BeTrue();

        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.Delete(path);
        });

        exception.Should().BeOfType<IOException>();
#if !NETFRAMEWORK
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Path information only included in exception message on Windows and not in .NET Framework
            exception.Should().BeOfType<IOException>()
               .Which.Message.Should().Contain($"'{Path.Combine(BasePath, path)}'");
        }
#endif
    }
}