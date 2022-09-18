using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.File;

public abstract partial class FileSystemMockDirectoryTests
{
    public IFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }
    public string BasePath { get; }

    protected FileSystemMockDirectoryTests(IFileSystem fileSystem, ITimeSystem timeSystem, string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
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

    [Theory, AutoData]
    public void CreationTime_ShouldBeSet(string path)
    {
        var start = TimeSystem.DateTime.Now.ApplySystemClockTolerance();
        var result = FileSystem.Directory.CreateDirectory(path);

        result.CreationTime.Should().BeOnOrAfter(start);
        result.CreationTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.CreationTime.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory, AutoData]
    public void CreationTimeUtc_ShouldBeSet(string path)
    {
        var start = TimeSystem.DateTime.UtcNow.ApplySystemClockTolerance();
        var result = FileSystem.Directory.CreateDirectory(path);

        result.CreationTimeUtc.Should().BeOnOrAfter(start);
        result.CreationTimeUtc.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.CreationTimeUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory, AutoData]
    public void LastAccessTime_ShouldBeSet(string path)
    {
        var start = TimeSystem.DateTime.Now.ApplySystemClockTolerance();
        var result = FileSystem.Directory.CreateDirectory(path);

        result.LastAccessTime.Should().BeOnOrAfter(start);
        result.LastAccessTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.LastAccessTime.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory, AutoData]
    public void LastAccessTimeUtc_ShouldBeSet(string path)
    {
        var start = TimeSystem.DateTime.UtcNow.ApplySystemClockTolerance();
        var result = FileSystem.Directory.CreateDirectory(path);

        result.LastAccessTimeUtc.Should().BeOnOrAfter(start);
        result.LastAccessTimeUtc.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.LastAccessTimeUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory, AutoData]
    public void LastAccessTime_CreateSubDirectory_ShouldUpdateLastAccessAndLastWriteTime(
        string path, string subPath)
    {
        var start = TimeSystem.DateTime.Now.ApplySystemClockTolerance();
        var result = FileSystem.Directory.CreateDirectory(path);
        TimeSystem.Thread.Sleep(100);
        var sleepTime = TimeSystem.DateTime.Now.ApplySystemClockTolerance();
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subPath));

        result.CreationTime.Should().BeOnOrAfter(start);
        result.CreationTime.Should().BeBefore(sleepTime);
        result.LastAccessTime.Should().BeOnOrAfter(sleepTime);
        result.LastAccessTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.LastWriteTime.Should().BeOnOrAfter(sleepTime);
        result.LastWriteTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
    }

    [Theory, AutoData]
    public void LastWriteTime_ShouldBeSet(string path)
    {
        var start = TimeSystem.DateTime.Now.ApplySystemClockTolerance();
        var result = FileSystem.Directory.CreateDirectory(path);

        result.LastWriteTime.Should().BeOnOrAfter(start);
        result.LastWriteTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.LastWriteTime.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory, AutoData]
    public void LastWriteTimeUtc_ShouldBeSet(string path)
    {
        var start = TimeSystem.DateTime.UtcNow.ApplySystemClockTolerance();
        var result = FileSystem.Directory.CreateDirectory(path);

        result.LastWriteTimeUtc.Should().BeOnOrAfter(start);
        result.LastWriteTimeUtc.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.LastWriteTimeUtc.Kind.Should().Be(DateTimeKind.Utc);
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

    [Theory, AutoData]
    public void CreateDirectory_ShouldCreateParentDirectories(string directoryLevel1, string directoryLevel2, string directoryLevel3)
    {
        var path =
            FileSystem.Path.Combine(directoryLevel1, directoryLevel2, directoryLevel3);

        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.Name.Should().Be(directoryLevel3);
        result.Exists.Should().BeTrue();
        result.ToString().Should().Be(path);
        result.Parent!.Name.Should().Be(directoryLevel2);
        result.Parent.Exists.Should().BeTrue();
        result.Parent.ToString().Should().Be(result.Parent.FullName);
        result.Parent.Parent!.Name.Should().Be(directoryLevel1);
        result.Parent.Parent.Exists.Should().BeTrue();
        result.Parent.Parent.ToString().Should().Be(result.Parent.Parent.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("/")]
    [InlineData("\\")]
    public void CreateDirectory_TrailingDirectorySeparator_ShouldNotBeTrimmed(
        string suffix)
    {
        string name = "foobar";
        string nameWithSuffix = "foobar" + suffix;

        IFileSystem.IDirectoryInfo result =
            FileSystem.Directory.CreateDirectory(nameWithSuffix);

        result.ToString().Should().Be(nameWithSuffix);
        result.Name.Should().Be(name);
        result.FullName.Should().Be(Path.Combine(BasePath, nameWithSuffix
           .TrimEnd(' ')
           .Replace(FileSystem.Path.AltDirectorySeparatorChar,
                FileSystem.Path.DirectorySeparatorChar)));
        FileSystem.Directory.Exists(name).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void Delete_FullPath_ShouldDeleteDirectory(string directoryName)
    {
        IFileSystem.IDirectoryInfo result =
            FileSystem.Directory.CreateDirectory(directoryName);

        FileSystem.Directory.Delete(result.FullName);

        bool exists = FileSystem.Directory.Exists(directoryName);

        exists.Should().BeFalse();
        result.Exists.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void Delete_ShouldDeleteDirectory(string directoryName)
    {
        IFileSystem.IDirectoryInfo result =
            FileSystem.Directory.CreateDirectory(directoryName);

        FileSystem.Directory.Delete(directoryName);

        bool exists = FileSystem.Directory.Exists(directoryName);

        exists.Should().BeFalse();
        result.Exists.Should().BeFalse();
    }
}