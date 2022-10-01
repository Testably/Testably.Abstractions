using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.EnumerateFiles))]
    public void
        EnumerateFiles_SearchOptionAllFiles_ShouldReturnAllFiles(
            string path)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.Initialize()
               .WithSubdirectory(path).Initialized(s => s
                   .WithASubdirectory().Initialized(d => d
                       .WithAFile()
                       .WithAFile())
                   .WithASubdirectory()
                   .WithAFile());
        IFileSystem.IDirectoryInfo baseDirectory =
            (IFileSystem.IDirectoryInfo)initialized[0];

        IFileSystem.IFileInfo[] result = baseDirectory
           .EnumerateFiles("*", SearchOption.AllDirectories).ToArray();

        result.Length.Should().Be(3);
        result.Should().Contain(d => d.Name == initialized[2].Name);
        result.Should().Contain(d => d.Name == initialized[3].Name);
        result.Should().Contain(d => d.Name == initialized[5].Name);
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
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.EnumerateFiles))]
    public void EnumerateFiles_SearchPattern_ShouldReturnExpectedValue(
        bool expectToBeFound, string searchPattern, string fileName)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithFile(fileName)
               .BaseDirectory;

        IFileSystem.IFileInfo[] result = baseDirectory
           .EnumerateFiles(searchPattern).ToArray();

        if (expectToBeFound)
        {
            result.Should().ContainSingle(d => d.Name == fileName,
                $"it should match '{searchPattern}'");
        }
        else
        {
            result.Should()
               .BeEmpty($"{fileName} should not match '{searchPattern}'");
        }
    }

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.EnumerateFiles))]
    public void
        EnumerateFiles_WithEnumerationOptions_ShouldConsiderSetOptions(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithASubdirectory().Initialized(s => s
                   .WithFile("xyz"))
               .WithAFile()
               .BaseDirectory;

        IFileSystem.IFileInfo[] result = baseDirectory
           .EnumerateFiles("XYZ",
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true,
                    // Filename could start with a leading '.' indicating it as Hidden in Linux
                    AttributesToSkip = FileAttributes.System
                }).ToArray();

        result.Length.Should().Be(1);
        result.Should().NotContain(d => d.Name == "foo");
        result.Should().Contain(d => d.Name == "xyz");
        result.Should().NotContain(d => d.Name == "bar");
    }
#endif

    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.EnumerateFiles))]
    public void EnumerateFiles_WithNewline_ShouldThrowArgumentException(
        string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.DirectoryInfo.New(path);
        string searchPattern = "foo\0bar";

        Exception? exception = Record.Exception(() =>
        {
            _ = baseDirectory.EnumerateFiles(searchPattern).FirstOrDefault();
        });

        exception.Should().BeOfType<ArgumentException>();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.EnumerateFiles))]
    public void
        EnumerateFiles_WithoutSearchString_ShouldReturnAllDirectFiles(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithFile("foo")
               .WithASubdirectory().Initialized(s => s
                   .WithFile("xyz"))
               .WithFile("bar")
               .BaseDirectory;

        IFileSystem.IFileInfo[] result = baseDirectory
           .EnumerateFiles().ToArray();

        result.Length.Should().Be(2);
        result.Should().Contain(d => d.Name == "foo");
        result.Should().NotContain(d => d.Name == "xyz");
        result.Should().Contain(d => d.Name == "bar");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.EnumerateFiles))]
    public void EnumerateFiles_WithSearchPattern_ShouldReturnMatchingFiles(
        string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithFile("foo")
               .WithFile("bar")
               .BaseDirectory;

        IEnumerable<IFileSystem.IFileInfo> result = baseDirectory
           .EnumerateFiles("foo").ToArray();

        result.Should().ContainSingle(d => d.Name == "foo");
        result.Count().Should().Be(1);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.EnumerateFiles))]
    public void
        EnumerateFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFiles(
            string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithASubdirectory().Initialized(s => s
                   .WithFile("xyz"))
               .WithASubdirectory().Initialized(s => s
                   .WithFile("xyz"))
               .WithSubdirectory("xyz").Initialized(s => s
                   .WithAFile())
               .BaseDirectory;

        IEnumerable<IFileSystem.IFileInfo> result = baseDirectory
           .EnumerateFiles("xyz", SearchOption.AllDirectories);

        result.Count().Should().Be(2);
    }
}