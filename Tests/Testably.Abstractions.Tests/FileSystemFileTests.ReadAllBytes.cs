using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    public void ReadAllBytes_MissingFile_ShouldThrowFileNotFoundException(string path)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.ReadAllBytes(path);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find file '{FileSystem.Path.GetFullPath(path)}'.");
    }

    [Theory]
    [AutoData]
    public void ReadAllBytes_ShouldReturnWrittenBytes(
        byte[] contents, string path)
    {
        FileSystem.File.WriteAllBytes(path, contents);

        byte[] result = FileSystem.File.ReadAllBytes(path);

        result.Should().BeEquivalentTo(contents);
    }
}