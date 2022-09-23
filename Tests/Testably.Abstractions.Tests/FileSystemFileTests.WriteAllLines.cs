namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    public void WriteAllLines_PreviousFile_ShouldOverwriteFileWithText(
        string path, string[] contents)
    {
        FileSystem.File.WriteAllText(path, "foo");

        FileSystem.File.WriteAllLines(path, contents);

        string[] result = FileSystem.File.ReadAllLines(path);
        result.Should().BeEquivalentTo(contents);
    }

    [Theory]
    [AutoData]
    public void WriteAllLines_ShouldCreateFileWithText(string path, string[] contents)
    {
        FileSystem.File.WriteAllLines(path, contents);

        string[] result = FileSystem.File.ReadAllLines(path);
        result.Should().BeEquivalentTo(contents);
    }
}