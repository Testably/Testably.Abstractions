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
        EnumerateFileSystemEntries_MissingDirectory_ShouldThrowDirectoryNotFoundException(
            string path)
    {
        string expectedPath = Path.Combine(BasePath, path);
        Exception? exception =
            Record.Exception(()
                => FileSystem.Directory.EnumerateFileSystemEntries(path).ToList());

        exception.Should().BeOfType<DirectoryNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find a part of the path '{expectedPath}'.");
        FileSystem.Directory.Exists(path).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void
        EnumerateFileSystemEntries_SearchOptionAllDirectories_FullPath_ShouldReturnAllSubdirectoriesWithFullPath(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar");

        List<string> result = FileSystem.Directory
           .EnumerateFileSystemEntries(baseDirectory.FullName, "*", SearchOption.AllDirectories)
           .ToList();

        result.Count.Should().Be(3);
        result.Should().Contain(FileSystem.Path.Combine(baseDirectory.FullName, "foo"));
        result.Should()
           .Contain(FileSystem.Path.Combine(baseDirectory.FullName, "foo", "xyz"));
        result.Should().Contain(FileSystem.Path.Combine(baseDirectory.FullName, "bar"));
    }

    [Theory]
    [AutoData]
    public void
        EnumerateFileSystemEntries_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar");

        List<string> result = FileSystem.Directory
           .EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories).ToList();

        result.Count.Should().Be(3);
        result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
        result.Should().Contain(FileSystem.Path.Combine(path, "foo", "xyz"));
        result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
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
    public void EnumerateFileSystemEntries_SearchPattern_ShouldReturnExpectedValue(
        bool expectToBeFound, string searchPattern, string subdirectoryName)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory("foo");
        baseDirectory.CreateSubdirectory(subdirectoryName);

        List<string> result = FileSystem.Directory
           .EnumerateFileSystemEntries("foo", searchPattern).ToList();

        if (expectToBeFound)
        {
            result.Should().ContainSingle(
                FileSystem.Path.Combine("foo", subdirectoryName),
                $"it should match {searchPattern}");
        }
        else
        {
            result.Should()
               .BeEmpty($"{subdirectoryName} should not match {searchPattern}");
        }
    }

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    [Theory]
    [AutoData]
    public void
        EnumerateFileSystemEntries_WithEnumerationOptions_ShouldConsiderSetOptions(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar");

        List<string> result = FileSystem.Directory
           .EnumerateFileSystemEntries(path, "XYZ",
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true
                }).ToList();

        result.Count.Should().Be(1);
        result.Should().NotContain(FileSystem.Path.Combine(path, "foo"));
        result.Should().Contain(FileSystem.Path.Combine(path, "foo", "xyz"));
        result.Should().NotContain(FileSystem.Path.Combine(path, "bar"));
    }
#endif

    [Theory]
    [AutoData]
    public void EnumerateFileSystemEntries_WithNewline_ShouldThrowArgumentException(
        string path)
    {
        string searchPattern = "foo\0bar";

        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.Directory.EnumerateFileSystemEntries(path, searchPattern)
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
        EnumerateFileSystemEntries_WithoutSearchString_ShouldReturnAllDirectSubdirectories(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar");
        FileSystem.File.WriteAllText(Path.Combine(path, "foo.txt"), "some content");

        List<string> result = FileSystem.Directory.EnumerateFileSystemEntries(path).ToList();

        result.Count.Should().Be(3);
        result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
        result.Should().Contain(FileSystem.Path.Combine(path, "foo.txt"));
        result.Should().NotContain(FileSystem.Path.Combine(path, "foo", "xyz"));
        result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
    }

    [Theory]
    [AutoData]
    public void EnumerateFileSystemEntries_WithSearchPattern_ShouldReturnMatchingSubdirectory(
        string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo");
        baseDirectory.CreateSubdirectory("bar");

        IEnumerable<string> result =
            FileSystem.Directory.EnumerateFileSystemEntries(path, "foo");

        result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
    }

    [Theory]
    [AutoData]
    public void
        EnumerateFileSystemEntries_WithSearchPatternInSubdirectory_ShouldReturnMatchingSubdirectory(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar/xyz");

        IEnumerable<string> result = FileSystem.Directory
           .EnumerateFileSystemEntries(path, "xyz", SearchOption.AllDirectories);

        result.Count().Should().Be(2);
    }
}