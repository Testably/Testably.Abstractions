using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CreateText))]
    public void CreateText_ShouldCreateFileIfMissing(
        string path, string appendText)
    {
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        using (StreamWriter stream = fileInfo.CreateText())
        {
            stream.Write(appendText);
        }

        string result = FileSystem.File.ReadAllText(path);

        result.Should().Be(appendText);
        FileSystem.File.Exists(path).Should().BeTrue();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CreateText))]
    public void CreateText_ShouldReplaceTextInExistingFile(
        string path, string contents, string appendText)
    {
        FileSystem.File.WriteAllText(path, contents);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        using (StreamWriter stream = fileInfo.CreateText())
        {
            stream.Write(appendText);
        }

        string result = FileSystem.File.ReadAllText(path);

        result.Should().Be(appendText);
    }
}