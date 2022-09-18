using FluentAssertions;
using System;
using System.Collections.Generic;
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
    public void CreateDirectory_ShouldCreateDirectory()
    {
        var result = FileSystem.Directory.CreateDirectory("foo");
        var exists = FileSystem.Directory.Exists("foo");

        exists.Should().BeTrue();
    }

    [Fact]
    public void CreateDirectory_Null_ShouldThrowArgumentNullException()
    {
        var exception =
            Record.Exception(() => FileSystem.Directory.CreateDirectory(null!));

        exception.Should().BeAssignableTo<ArgumentNullException>().Which.ParamName.Should().Be("path");
    }

    [Fact]
    public void CreateDirectory_Empty_ShouldThrowArgumentException()
    {
        var exception =
            Record.Exception(() => FileSystem.Directory.CreateDirectory(string.Empty));

        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.ParamName.Should().Be("path");
        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.Message.Should().Be("Path cannot be the empty string or all whitespace. (Parameter 'path')");
    }

    [Fact]
    public void CreateDirectory_IllegalCharacters_ShouldThrowArgumentException()
    {
        foreach (var c in FileSystem.Path.GetInvalidPathChars().Where(c => c != '\0'))
        {
            var path = "foo" + c + "bar";
            var expectedMessage =
                $"The filename, directory name, or volume label syntax is incorrect. : '{Path.Combine(BasePath, path)}'";
            var exception =
                Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

            exception.Should().BeAssignableTo<IOException>()
               .Which.Message.Should().Be(expectedMessage);
        }
    }

    [Fact]
    public void CreateDirectory_NullCharacter_ShouldThrowArgumentException()
    {
        var path = "foo\0bar";
        var expectedMessage =
            $"Illegal characters in path. (Parameter 'path')";
        var exception =
            Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.Message.Should().Be(expectedMessage);
    }

    
}