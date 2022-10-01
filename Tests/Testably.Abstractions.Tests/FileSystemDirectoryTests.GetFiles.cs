using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void
        GetFiles_MissingDirectory_ShouldThrowDirectoryNotFoundException(
            string path)
    {
        string expectedPath = Path.Combine(BasePath, path);
        Exception? exception =
            Record.Exception(()
                => FileSystem.Directory.GetFiles(path).ToList());

        exception.Should().BeOfType<DirectoryNotFoundException>()
           .Which.Message.Should().Contain($"'{expectedPath}'.");
        FileSystem.Directory.Exists(path).Should().BeFalse();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void
        GetFiles_SearchOptionAllDirectories_FullPath_ShouldReturnAllFilesWithFullPath(
            string path)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.InitializeIn(path)
               .WithAFile()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile());

        List<string> result = FileSystem.Directory
           .GetFiles(FileSystem.Path.GetFullPath(path), "*",
                SearchOption.AllDirectories)
           .ToList();

        result.Count.Should().Be(2);
        result.Should().Contain(initialized[0].FullName);
        result.Should().Contain(initialized[2].FullName);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void GetFiles_SearchOptionAllDirectories_ShouldReturnAllFiles(
        string path)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.InitializeIn(path)
               .WithAFile()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile());

        List<string> result = FileSystem.Directory
           .GetFiles(path, "*", SearchOption.AllDirectories)
           .ToList();

        result.Count.Should().Be(2);
        result.Should().Contain(initialized[0].ToString());
        result.Should().Contain(initialized[2].ToString());
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
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void GetFiles_SearchPattern_ShouldReturnExpectedValue(
        bool expectToBeFound, string searchPattern, string fileName)
    {
        FileSystem.Initialize().WithFile(fileName);

        List<string> result = FileSystem.Directory
           .GetFiles(".", searchPattern).ToList();

        if (expectToBeFound)
        {
            result.Should().ContainSingle(
                fileName,
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
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void
        GetFiles_WithEnumerationOptions_ShouldConsiderSetOptions(
            string path)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.InitializeIn(path)
               .WithAFile()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile());

        List<string> result = FileSystem.Directory
           .GetFiles(path, initialized[2].Name.ToUpper(),
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true,
                    // Filename could start with a leading '.' indicating it as Hidden in Linux
                    AttributesToSkip = FileAttributes.System
                }).ToList();

        result.Count.Should().Be(1);
        result.Should().NotContain(initialized[0].ToString());
        result.Should().Contain(initialized[2].ToString());
    }
#endif

    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void GetFiles_WithNewline_ShouldThrowArgumentException(
        string path)
    {
        string searchPattern = "foo\0bar";

        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.Directory.GetFiles(path, searchPattern)
               .FirstOrDefault();
        });

#if NETFRAMEWORK
        // The searchPattern is not included in .NET Framework
        exception.Should().BeOfType<ArgumentException>();
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should().Contain($"'{searchPattern}'");
#endif
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void
        GetFiles_WithoutSearchString_ShouldReturnAllFilesInDirectSubdirectories(
            string path)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.InitializeIn(path)
               .WithAFile()
               .WithAFile()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile());

        List<string> result = FileSystem.Directory.GetFiles(path).ToList();

        result.Count.Should().Be(2);
        result.Should().Contain(initialized[0].ToString());
        result.Should().Contain(initialized[1].ToString());
        result.Should().NotContain(initialized[3].ToString());
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void GetFiles_WithSearchPattern_ShouldReturnMatchingFiles(
        string path)
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.InitializeIn(path)
               .WithAFile()
               .WithAFile()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile());

        List<string> result = FileSystem.Directory
           .GetFiles(path, initialized[0].Name)
           .ToList();

        result.Count.Should().Be(1);
        result.Should().Contain(initialized[0].ToString());
        result.Should().NotContain(initialized[1].ToString());
        result.Should().NotContain(initialized[3].ToString());
    }

    [Fact]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetFiles))]
    public void
        GetFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFilesInSubdirectories()
    {
        FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
            FileSystem.Initialize()
               .WithASubdirectory().Initialized(s => s
                   .WithAFile("foobar"))
               .WithASubdirectory().Initialized(s => s
                   .WithAFile("foobar"))
               .WithASubdirectory().Initialized(s => s
                   .WithAFile());

        IEnumerable<string> result = FileSystem.Directory
           .GetFiles(".\\.", "*.foobar", SearchOption.AllDirectories)
           .ToArray();

        result.Count().Should().Be(2);
        result.Should().Contain(initialized[1].ToString());
        result.Should().Contain(initialized[3].ToString());
    }
}