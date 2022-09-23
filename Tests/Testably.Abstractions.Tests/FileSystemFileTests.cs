using System.Collections.Generic;
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
    public void AppendAllLines_ExistingFile_ShouldAppendLinesToFile(
        string path, List<string> previousContents, List<string> contents)
    {
        FileSystem.File.AppendAllLines(path, previousContents);

        FileSystem.File.AppendAllLines(path, contents);

        FileSystem.File.Exists(path).Should().BeTrue();
        FileSystem.File.ReadAllLines(path).Should()
           .BeEquivalentTo(previousContents.Concat(contents));
    }

    [Theory]
    [AutoData]
    public void AppendAllLines_MissingFile_ShouldCreateFile(
        string path, List<string> contents)
    {
        FileSystem.File.AppendAllLines(path, contents);

        FileSystem.File.Exists(path).Should().BeTrue();
        FileSystem.File.ReadAllLines(path).Should().BeEquivalentTo(contents);
    }

    [Theory]
    [AutoData]
    public void AppendAllLines_ShouldEndWithNewline(string path)
    {
        string[] contents = { "foo", "bar" };
        string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

        FileSystem.File.AppendAllLines(path, contents);

        FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [MemberAutoData(nameof(GetEncodingDifference))]
    public void AppendAllLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
        string specialLine, Encoding writeEncoding, Encoding readEncoding,
        string path, string[] contents)
    {
        contents[1] = specialLine;
        FileSystem.File.AppendAllLines(path, contents, writeEncoding);

        string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

        result.Should().NotBeEquivalentTo(contents,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
        result[0].Should().Be(contents[0]);
    }

    [Theory]
    [AutoData]
    public void ReadAllLines_MissingFile_ShouldThrowFileNotFoundException(string path)
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.File.ReadAllLines(path);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find file '{FileSystem.Path.GetFullPath(path)}'.");
    }

    [Theory]
    [AutoData]
    public void ReadAllLines_ShouldEnumerateLines(string path, string[] lines)
    {
        string contents = string.Join(Environment.NewLine, lines);
        FileSystem.File.WriteAllText(path, contents);

        string[] results = FileSystem.File.ReadAllLines(path);

        results.Should().BeEquivalentTo(lines);
    }

    [Theory]
    [MemberAutoData(nameof(GetEncodingDifference))]
    public void ReadAllLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
        string specialLine, Encoding writeEncoding, Encoding readEncoding,
        string path, string[] lines)
    {
        lines[1] = specialLine;
        string contents = string.Join(Environment.NewLine, lines);
        FileSystem.File.WriteAllText(path, contents, writeEncoding);

        string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

        result.Should().NotBeEquivalentTo(lines,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
        result[0].Should().Be(lines[0]);
    }

    [Theory]
    [AutoData]
    public void ReadAllText_MissingFile_ShouldThrowFileNotFoundException(string path)
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
    [MemberAutoData(nameof(GetEncodingDifference))]
    public void ReadAllText_WithDifferentEncoding_ShouldNotReturnWrittenText(
        string contents, Encoding writeEncoding, Encoding readEncoding, string path)
    {
        FileSystem.File.WriteAllText(path, contents, writeEncoding);

        string result = FileSystem.File.ReadAllText(path, readEncoding);

        result.Should().NotBe(contents,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
    }

    [Theory]
    [AutoData]
    public void ReadLines_MissingFile_ShouldThrowFileNotFoundException(string path)
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
    [MemberAutoData(nameof(GetEncodingDifference))]
    public void ReadLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
        string specialLine, Encoding writeEncoding, Encoding readEncoding,
        string path, string[] lines)
    {
        lines[1] = specialLine;
        string contents = string.Join(Environment.NewLine, lines);
        FileSystem.File.WriteAllText(path, contents, writeEncoding);

        string[] result = FileSystem.File.ReadLines(path, readEncoding).ToArray();

        result.Should().NotBeEquivalentTo(lines,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
        result[0].Should().Be(lines[0]);
    }

    [Theory]
    [AutoData]
    public void WriteAllText_PreviousFile_ShouldOverwriteFileWithText(
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, "foo");
        FileSystem.File.WriteAllText(path, contents);

        string result = FileSystem.File.ReadAllText(path);

        result.Should().Be(contents);
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

    #region Helpers

    private static IEnumerable<object[]> GetEncodingDifference()
    {
        yield return new object[]
        {
            SpecialCharactersContent, Encoding.ASCII, Encoding.UTF8
        };
    }

    #endregion
}