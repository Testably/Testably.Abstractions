using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    public void
        EnumerateFiles_MissingDirectory_ShouldThrowDirectoryNotFoundException(
            string path)
    {
        string expectedPath = Path.Combine(BasePath, path);
        Exception? exception =
            Record.Exception(()
                => FileSystem.Directory.EnumerateFiles(path).ToList());

        exception.Should().BeOfType<DirectoryNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find a part of the path '{expectedPath}'.");
        FileSystem.Directory.Exists(path).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void
        EnumerateFiles_SearchOptionAllDirectories_FullPath_ShouldReturnAllSubdirectoriesWithFullPath(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        var subDirectory = baseDirectory.CreateSubdirectory("foo");
        FileSystem.File.WriteAllText(Path.Combine(baseDirectory.FullName, "xyz.txt"), "some content");
        FileSystem.File.WriteAllText(Path.Combine(subDirectory.FullName, "bar.json"), "{}");

        List<string> result = FileSystem.Directory
           .EnumerateFiles(baseDirectory.FullName, "*", SearchOption.AllDirectories)
           .ToList();

        result.Count.Should().Be(2);
        result.Should().Contain(FileSystem.Path.Combine(baseDirectory.FullName, "xyz.txt"));
        result.Should()
           .Contain(FileSystem.Path.Combine(subDirectory.FullName, "bar.json"));
    }

    [Theory]
    [AutoData]
    public void
        EnumerateFiles_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        var subDirectory = baseDirectory.CreateSubdirectory("foo");
        FileSystem.File.WriteAllText(Path.Combine(baseDirectory.FullName, "xyz.txt"), "some content");
        FileSystem.File.WriteAllText(Path.Combine(subDirectory.FullName, "bar.json"), "{}");

        List<string> result = FileSystem.Directory
           .EnumerateFiles(path, "*", SearchOption.AllDirectories)
           .ToList();

        result.Count.Should().Be(2);
        result.Should().Contain(FileSystem.Path.Combine(path, "xyz.txt"));
        result.Should()
           .Contain(FileSystem.Path.Combine(path, "foo", "bar.json"));
    }

    [Theory]
#if NETFRAMEWORK
    [InlineAutoData(false, "")]
#else
    [InlineAutoData(true, "")]
#endif
    [InlineAutoData(true, "*")]
    [InlineAutoData(true, ".")]
    [InlineAutoData(true, "*.*")]
    [InlineData(true, "a*c", "abc")]
    [InlineData(true, "ab*c", "abc")]
    [InlineData(true, "abc?", "abc")]
    [InlineData(false, "ab?c", "abc")]
    [InlineData(false, "ac", "abc")]
    public void EnumerateFiles_SearchPattern_ShouldReturnExpectedValue(
        bool expectToBeFound, string searchPattern, string fileName)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory("foo");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(baseDirectory.FullName, fileName), "some content");

        List<string> result = FileSystem.Directory
           .EnumerateFiles("foo", searchPattern).ToList();

        if (expectToBeFound)
        {
            result.Should().ContainSingle(
                FileSystem.Path.Combine("foo", fileName),
                $"it should match {searchPattern}");
        }
        else
        {
            result.Should()
               .BeEmpty($"{fileName} should not match {searchPattern}");
        }
    }

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    [Theory]
    [AutoData]
    public void
        EnumerateFiles_WithEnumerationOptions_ShouldConsiderSetOptions(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        var subdirectory = baseDirectory.CreateSubdirectory("foo");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(subdirectory.FullName, "xyz.txt"), "some content");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(baseDirectory.FullName, "bar.txt"), "some other content");

        List<string> result = FileSystem.Directory
           .EnumerateFiles(path, "XYZ.txt",
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true
                }).ToList();

        result.Count.Should().Be(1);
        result.Should().NotContain(FileSystem.Path.Combine(path, "foo"));
        result.Should().Contain(FileSystem.Path.Combine(path, "foo", "xyz.txt"));
        result.Should().NotContain(FileSystem.Path.Combine(path, "bar.txt"));
    }
#endif

    [Theory]
    [AutoData]
    public void EnumerateFiles_WithNewline_ShouldThrowArgumentException(
        string path)
    {
        string searchPattern = "foo\0bar";

        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.Directory.EnumerateFiles(path, searchPattern)
               .FirstOrDefault();
        });

#if NETFRAMEWORK
        // The searchPattern is not included in .NET Framework
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should().Contain("Illegal characters in path");
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should().Contain("Illegal characters in path")
           .And.Contain($" (Parameter '{searchPattern}')");
#endif
    }

    [Theory]
    [AutoData]
    public void
        EnumerateFiles_WithoutSearchString_ShouldReturnAllDirectSubdirectories(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        var subdirectory = baseDirectory.CreateSubdirectory("foo");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(subdirectory.FullName, "not-found.txt"), "some content");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(baseDirectory.FullName, "found.txt"), "some other content");
        
        List<string> result = FileSystem.Directory.EnumerateFiles(path).ToList();

        result.Count.Should().Be(1);
        result.Should().Contain(FileSystem.Path.Combine(path, "found.txt"));
        result.Should().NotContain(FileSystem.Path.Combine(path, "foo", "not-found.txt"));
    }

    [Theory]
    [AutoData]
    public void EnumerateFiles_WithSearchPattern_ShouldReturnMatchingSubdirectory(
        string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "foo"), "some content");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "bar"), "some content");

        IEnumerable<string> result =
            FileSystem.Directory.EnumerateFiles(path, "foo");

        result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
    }

    [Theory]
    [AutoData]
    public void
        EnumerateFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingSubdirectory(
            string path1, string path2)
    {
        FileSystem.Directory.CreateDirectory(path1);
        FileSystem.Directory.CreateDirectory(path2);
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(path1, "xyz.txt"), "some content");
        FileSystem.File.WriteAllText(FileSystem.Path.Combine(path2, "xyz.txt"), "some content");

        IEnumerable<string> result = FileSystem.Directory
           .EnumerateFiles(".", "xyz.txt", SearchOption.AllDirectories);

        result.Count().Should().Be(2);
    }
}