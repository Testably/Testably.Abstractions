using System.IO;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    private const string SpecialCharactersContent = "_€_Ä_Ö_Ü";

    public string BasePath { get; }

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileTests(
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
    public void ReadAllText_MissingFile_ShouldThrow(string path)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.ReadAllText(path);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find file '{FileSystem.Path.GetFullPath(path)}'.");
    }

    [Theory]
    [AutoData]
    public void
        ReadAllText_WithDifferentEncoding_ShouldNotReturnWrittenText(
            string path)
    {
        string contents = SpecialCharactersContent;
        Encoding writeEncoding = Encoding.ASCII;
        Encoding readEncoding = Encoding.UTF8;
        FileSystem.File.WriteAllText(path, contents, writeEncoding);

        string result = FileSystem.File.ReadAllText(path, readEncoding);

        result.Should().NotBe(contents,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
    }

    [Theory]
    [AutoData]
    public void ReadLines_MissingFile_ShouldThrow(string path)
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.File.ReadLines(path).FirstOrDefault();
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find file '{FileSystem.Path.GetFullPath(path)}'.");
    }

    [Theory]
    [AutoData]
    public void ReadLines_ShouldEnumerateLines(string path, string[] lines)
    {
        string contents = string.Join(Environment.NewLine, lines);
        FileSystem.File.WriteAllText(path, contents);

        string[] results = FileSystem.File.ReadLines(path).ToArray();

        results.Should().BeEquivalentTo(lines);
    }

    [Theory]
    [AutoData]
    public void
        ReadLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
            string path, string[] lines)
    {
        lines[1] = SpecialCharactersContent;
        string contents = string.Join(Environment.NewLine, lines);
        Encoding writeEncoding = Encoding.ASCII;
        Encoding readEncoding = Encoding.UTF8;
        FileSystem.File.WriteAllText(path, contents, writeEncoding);

        string[] result = FileSystem.File.ReadLines(path, readEncoding).ToArray();

        result.Should().NotBeEquivalentTo(lines,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
        result[0].Should().Be(lines[0]);
    }

    [Theory]
    [AutoData]
    public void WriteAllText_ShouldCreateFileWithText(string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        string result = FileSystem.File.ReadAllText(path);

        result.Should().Be(contents);
    }

    [Theory]
    [AutoData]
    public void WriteAllText_SpecialCharacters_ShouldReturnSameText(string path)
    {
        char[] specialCharacters = { 'Ä', 'Ö', 'Ü', 'ä', 'ö', 'ü', 'ß' };
        foreach (char specialCharacter in specialCharacters)
        {
            string contents = "_" + specialCharacter;
            FileSystem.File.WriteAllText(path, contents);

            string result = FileSystem.File.ReadAllText(path);

            result.Should().Be(contents,
                $"{contents} should be encoded and decoded identical.");
        }
    }
}