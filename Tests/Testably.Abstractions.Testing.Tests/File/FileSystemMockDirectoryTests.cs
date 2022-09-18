using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.File;

public abstract partial class FileSystemMockDirectoryTests
{
    public IFileSystem FileSystem { get; }
    public string BasePath { get; }

    protected FileSystemMockDirectoryTests(IFileSystem fileSystem, string basePath)
    {
        FileSystem = fileSystem;
        BasePath = basePath;
    }

    [Fact]
    public void CreateDirectory_Empty_ShouldThrowArgumentException()
    {
        Exception? exception =
            Record.Exception(() => FileSystem.Directory.CreateDirectory(string.Empty));

        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.ParamName.Should().Be("path");
        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.Message.Should()
           .Be("Path cannot be the empty string or all whitespace. (Parameter 'path')");
    }

    [Fact]
    public void CreateDirectory_IllegalCharacters_ShouldThrowArgumentException()
    {
        foreach (char c in FileSystem.Path.GetInvalidPathChars().Where(c => c != '\0'))
        {
            string path = "foo" + c + "bar";
            string expectedMessage =
                $"The filename, directory name, or volume label syntax is incorrect. : '{Path.Combine(BasePath, path)}'";
            Exception? exception =
                Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

            exception.Should().BeAssignableTo<IOException>()
               .Which.Message.Should().Be(expectedMessage);
        }
    }

    [Fact]
    public void CreateDirectory_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception =
            Record.Exception(() => FileSystem.Directory.CreateDirectory(null!));

        exception.Should().BeAssignableTo<ArgumentNullException>().Which.ParamName
           .Should().Be("path");
    }

    [Fact]
    public void CreateDirectory_NullCharacter_ShouldThrowArgumentException()
    {
        string path = "foo\0bar";
        string expectedMessage =
            "Illegal characters in path. (Parameter 'path')";
        Exception? exception =
            Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void CreateDirectory_ShouldCreateDirectoryInBasePath()
    {
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory("foo");
        bool exists = FileSystem.Directory.Exists("foo");

        exists.Should().BeTrue();
        result.FullName.Should().StartWith(BasePath);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("/")]
    [InlineData("\\")]
    public void CreateDirectory_TrailingDirectorySeparator_ShouldNotBeTrimmed(string suffix)
    {
        string name = "foobar";
        string nameWithSuffix = "foobar" + suffix;

        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(nameWithSuffix);

        result.ToString().Should().Be(nameWithSuffix);
        result.Name.Should().Be(name);
        result.FullName.Should().Be(Path.Combine(BasePath, nameWithSuffix
           .TrimEnd(' ')
           .Replace(FileSystem.Path.AltDirectorySeparatorChar, FileSystem.Path.DirectorySeparatorChar)));
        FileSystem.Directory.Exists(name).Should().BeTrue();
    }
}