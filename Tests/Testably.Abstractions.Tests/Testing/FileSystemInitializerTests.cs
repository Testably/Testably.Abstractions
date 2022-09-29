using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.Testing;

public class FileSystemInitializerTests
{

    [Fact]

    public void X()
    {
        var drives = DriveInfo.GetDrives();
        var logicalDrives = Directory.GetLogicalDrives();
        foreach (var drive in logicalDrives)
        {
            var driveInfo = new DriveInfo(drive);
        }
    }
    [Fact]
    public void Initialize_WithAFile_ShouldCreateFile()
    {
        FileSystemMock sut = new();
        sut.Initialize().WithAFile();

        sut.Directory.EnumerateFiles(".").Should().ContainSingle();
    }

    [Theory]
    [AutoData]
    public void Initialize_WithAFile_WithExtension_ShouldCreateFileWithExtension(
        string extension)
    {
        FileSystemMock sut = new();
        sut.Initialize().WithAFile(extension);

        sut.Directory.EnumerateFiles(".", $"*.{extension}").Should().ContainSingle();
    }

    [Fact]
    public void Initialize_WithASubdirectory_ShouldCreateDirectory()
    {
        FileSystemMock sut = new();
        sut.Initialize().WithASubdirectory();

        sut.Directory.EnumerateDirectories(".").Should().ContainSingle();
    }

    [Theory]
    [AutoData]
    public void Initialize_WithFile_Existing_ShouldThrowTestingException(string fileName)
    {
        FileSystemMock sut = new();
        sut.File.WriteAllText(fileName, null);
        Exception? exception = Record.Exception(() =>
        {
            sut.Initialize().WithFile(fileName);
        });

        exception.Should().BeOfType<FileSystemInitializer.TestingException>()
           .Which.Message.Should().Contain(fileName);
    }

    [Theory]
    [AutoData]
    public void Initialize_WithFile_HasBytesContent_ShouldCreateFileWithGivenFileContent(
        string fileName, byte[] fileContent)
    {
        FileSystemMock sut = new();
        sut.Initialize()
           .WithFile(fileName).Which(f => f
               .HasBytesContent(fileContent));

        byte[] result = sut.File.ReadAllBytes(fileName);

        result.Should().BeEquivalentTo(fileContent);
    }

    [Theory]
    [AutoData]
    public void Initialize_WithFile_HasStringContent_ShouldCreateFileWithGivenFileContent(
        string fileName, string fileContent)
    {
        FileSystemMock sut = new();
        sut.Initialize()
           .WithFile(fileName).Which(f => f
               .HasStringContent(fileContent));

        string result = sut.File.ReadAllText(fileName);

        result.Should().Be(fileContent);
    }

    [Theory]
    [AutoData]
    public void Initialize_WithFile_ShouldCreateFileWithGivenFileName(string fileName)
    {
        FileSystemMock sut = new();
        sut.Initialize().WithFile(fileName);

        sut.Directory.EnumerateFiles(".", fileName).Should().ContainSingle();
    }

    [Fact]
    public void Initialize_WithNestedSubdirectories_ShouldCreateAllNestedDirectories()
    {
        FileSystemMock sut = new();
        sut.Initialize()
           .WithSubdirectory("foo").Initialized(d => d
               .WithSubdirectory("bar").Initialized(s => s
                   .WithSubdirectory("xyz")));

        List<string> result = sut.Directory
           .EnumerateDirectories(".", "*", SearchOption.AllDirectories).ToList();

        result.Count.Should().Be(3);
        result.Should().Contain("foo");
        result.Should().Contain(sut.Path.Combine("foo", "bar"));
        result.Should().Contain(sut.Path.Combine("foo", "bar", "xyz"));
    }

    [Theory]
    [AutoData]
    public void Initialize_WithSubdirectory_Existing_ShouldThrowTestingException(
        string directoryName)
    {
        FileSystemMock sut = new();
        sut.Directory.CreateDirectory(directoryName);
        Exception? exception = Record.Exception(() =>
        {
            sut.Initialize().WithSubdirectory(directoryName);
        });

        exception.Should().BeOfType<FileSystemInitializer.TestingException>()
           .Which.Message.Should().Contain(directoryName);
    }

    [Theory]
    [AutoData]
    public void Initialize_WithSubdirectory_ShouldCreateDirectoryWithGivenDirectoryName(
        string directoryName)
    {
        FileSystemMock sut = new();
        sut.Initialize().WithSubdirectory(directoryName);

        sut.Directory.EnumerateDirectories(".", directoryName).Should().ContainSingle();
    }
}