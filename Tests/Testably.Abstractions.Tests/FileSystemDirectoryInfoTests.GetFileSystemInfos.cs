using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.GetFileSystemInfos))]
    public void
        GetFileSystemInfos_SearchOptionAllFiles_ShouldReturnAllFiles(
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

        IFileSystem.IFileSystemInfo[] result = baseDirectory
           .GetFileSystemInfos("*", SearchOption.AllDirectories);

        result.Length.Should().Be(5);
        result.Should().Contain(d => d.Name == initialized[1].Name);
        result.Should().Contain(d => d.Name == initialized[2].Name);
        result.Should().Contain(d => d.Name == initialized[3].Name);
        result.Should().Contain(d => d.Name == initialized[4].Name);
        result.Should().Contain(d => d.Name == initialized[5].Name);
    }

    [SkippableTheory]
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
        nameof(IFileSystem.IDirectoryInfo.GetFileSystemInfos))]
    public void GetFileSystemInfos_SearchPattern_ShouldReturnExpectedValue(
        bool expectToBeFound, string searchPattern, string fileName)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithFile(fileName)
               .BaseDirectory;

        IFileSystem.IFileSystemInfo[] result = baseDirectory
           .GetFileSystemInfos(searchPattern);

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
    [SkippableFact]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.GetFileSystemInfos))]
    public void
        GetFileSystemInfos_WithEnumerationOptions_ShouldConsiderSetOptions()
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithASubdirectory().Initialized(s => s
                   .WithFile("xyz"))
               .WithAFile()
               .BaseDirectory;

        IFileSystem.IFileSystemInfo[] result = baseDirectory
           .GetFileSystemInfos("XYZ",
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true,
                    // Filename could start with a leading '.' indicating it as Hidden in Linux
                    AttributesToSkip = FileAttributes.System
                });

        result.Length.Should().Be(1);
        result.Should().NotContain(d => d.Name == "foo");
        result.Should().Contain(d => d.Name == "xyz");
        result.Should().NotContain(d => d.Name == "bar");
    }
#endif

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.GetFileSystemInfos))]
    public void GetFileSystemInfos_WithNewline_ShouldThrowArgumentException(
        string path)
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.DirectoryInfo.New(path);
        string searchPattern = "foo\0bar";

        Exception? exception = Record.Exception(() =>
        {
            _ = baseDirectory.GetFileSystemInfos(searchPattern).FirstOrDefault();
        });

        exception.Should().BeOfType<ArgumentException>();
    }

    [SkippableFact]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.GetFileSystemInfos))]
    public void
        GetFileSystemInfos_WithoutSearchString_ShouldReturnAllDirectFilesAndDirectories()
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithFile("foo")
               .WithSubdirectory("muh").Initialized(s => s
                   .WithFile("xyz"))
               .WithFile("bar")
               .BaseDirectory;

        IFileSystem.IFileSystemInfo[] result = baseDirectory
           .GetFileSystemInfos();

        result.Length.Should().Be(3);
        result.Should().Contain(d => d.Name == "foo");
        result.Should().Contain(d => d.Name == "muh");
        result.Should().NotContain(d => d.Name == "xyz");
        result.Should().Contain(d => d.Name == "bar");
    }

    [SkippableFact]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.GetFileSystemInfos))]
    public void GetFileSystemInfos_WithSearchPattern_ShouldReturnMatchingFiles()
    {
        IFileSystem.IDirectoryInfo baseDirectory =
            FileSystem.Initialize()
               .WithFile("foo")
               .WithFile("bar")
               .BaseDirectory;

        IEnumerable<IFileSystem.IFileSystemInfo> result = baseDirectory
           .GetFileSystemInfos("foo");

        result.Should().ContainSingle(d => d.Name == "foo");
        result.Count().Should().Be(1);
    }

    [SkippableFact]
    [FileSystemTests.DirectoryInfo(
        nameof(IFileSystem.IDirectoryInfo.GetFileSystemInfos))]
    public void
        GetFileSystemInfos_WithSearchPatternInSubdirectory_ShouldReturnMatchingFiles()
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

        IEnumerable<IFileSystem.IFileSystemInfo> result = baseDirectory
           .GetFileSystemInfos("xyz", SearchOption.AllDirectories);

        result.Count().Should().Be(3);
    }
}