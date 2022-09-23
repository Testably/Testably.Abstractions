using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    public void
        EnumerateDirectories_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar");

        List<IFileSystem.IDirectoryInfo> result = baseDirectory
           .EnumerateDirectories("*", SearchOption.AllDirectories).ToList();

        result.Count.Should().Be(3);
        result.Should().Contain(d => d.Name == "foo");
        result.Should().Contain(d => d.Name == "bar");
        result.Should().Contain(d => d.Name == "xyz");
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
    public void EnumerateDirectories_SearchPattern_ShouldReturnExpectedValue(
        bool expectToBeFound, string searchPattern, string subdirectoryName)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory("foo");
        baseDirectory.CreateSubdirectory(subdirectoryName);

        List<IFileSystem.IDirectoryInfo> result = baseDirectory
           .EnumerateDirectories(searchPattern).ToList();

        if (expectToBeFound)
        {
            result.Should().ContainSingle(d => d.Name == subdirectoryName,
                $"it should match '{searchPattern}'");
        }
        else
        {
            result.Should()
               .BeEmpty($"{subdirectoryName} should not match '{searchPattern}'");
        }
    }

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    [Theory]
    [AutoData]
    public void
        EnumerateDirectories_WithEnumerationOptions_ShouldConsiderSetOptions(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar");

        List<IFileSystem.IDirectoryInfo> result = baseDirectory
           .EnumerateDirectories("XYZ",
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true
                }).ToList();

        result.Count.Should().Be(1);
        result.Should().NotContain(d => d.Name == "foo");
        result.Should().Contain(d => d.Name == "xyz");
        result.Should().NotContain(d => d.Name == "bar");
    }
#endif

    [Theory]
    [AutoData]
    public void EnumerateDirectories_WithNewline_ShouldThrowArgumentException(
        string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.DirectoryInfo.New(path);
        string searchPattern = "foo\0bar";

        Exception? exception = Record.Exception(() =>
        {
            _ = baseDirectory.EnumerateDirectories(searchPattern).FirstOrDefault();
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
        EnumerateDirectories_WithoutSearchString_ShouldReturnAllDirectSubdirectories(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar");

        List<IFileSystem.IDirectoryInfo> result = baseDirectory
           .EnumerateDirectories().ToList();

        result.Count.Should().Be(2);
        result.Should().Contain(d => d.Name == "foo");
        result.Should().NotContain(d => d.Name == "xyz");
        result.Should().Contain(d => d.Name == "bar");
    }

    [Theory]
    [AutoData]
    public void EnumerateDirectories_WithSearchPattern_ShouldReturnMatchingSubdirectory(
        string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo");
        baseDirectory.CreateSubdirectory("bar");

        IEnumerable<IFileSystem.IDirectoryInfo> result = baseDirectory
           .EnumerateDirectories("foo");

        result.Should().ContainSingle(d => d.Name == "foo");
    }

    [Theory]
    [AutoData]
    public void
        EnumerateDirectories_WithSearchPatternInSubdirectory_ShouldReturnMatchingSubdirectory(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Directory.CreateDirectory(path);
        baseDirectory.CreateSubdirectory("foo/xyz");
        baseDirectory.CreateSubdirectory("bar/xyz");

        IEnumerable<IFileSystem.IDirectoryInfo> result = baseDirectory
           .EnumerateDirectories("xyz", SearchOption.AllDirectories);

        result.Count().Should().Be(2);
    }
}