using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.OpenText))]
    public void OpenText_MissingFile_ShouldThrowFileNotFoundException(
        string path)
    {
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        Exception? exception = Record.Exception(() =>
        {
            using StreamReader stream = fileInfo.OpenText();
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.OpenText))]
    public void OpenText_ShouldReturnFileContent(
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        using StreamReader stream = fileInfo.OpenText();

        string result = stream.ReadToEnd();
        result.Should().Be(contents);
    }
}