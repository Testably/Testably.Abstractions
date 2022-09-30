using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_ShouldSetCreationTime(string path)
    {
        DateTime start = TimeSystem.DateTime.Now;

        FileSystem.Directory.CreateDirectory(path);

        DateTime result = FileSystem.Directory.GetCreationTime(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_ShouldSetCreationTimeUtc(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;

        FileSystem.Directory.CreateDirectory(path);

        DateTime result = FileSystem.Directory.GetCreationTimeUtc(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_Empty_ShouldThrowArgumentException()
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.CreateDirectory(string.Empty);
        });

#if NETFRAMEWORK
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should()
           .Be("Path cannot be the empty string or all whitespace.");
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.ParamName.Should().Be("path");
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should()
           .Be("Path cannot be the empty string or all whitespace. (Parameter 'path')");
#endif
    }

    [Fact]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_IllegalCharacters_ShouldThrowArgumentException()
    {
        foreach (char c in FileSystem.Path.GetInvalidPathChars().Where(c => c != '\0'))
        {
            string path = "foo" + c + "bar";
            Exception? exception = Record.Exception(() =>
            {
                FileSystem.Directory.CreateDirectory(path);
            });

#if NETFRAMEWORK
            exception.Should().BeOfType<ArgumentException>();
#else
            string expectedMessage = $"'{Path.Combine(BasePath, path)}'";
            exception.Should().BeOfType<IOException>()
               .Which.Message.Should().Contain(expectedMessage);
#endif
        }
    }

    [Fact]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception =
            Record.Exception(() => FileSystem.Directory.CreateDirectory(null!));

        exception.Should().BeOfType<ArgumentNullException>().Which.ParamName
           .Should().Be("path");
    }

    [Fact]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_NullCharacter_ShouldThrowArgumentException()
    {
        string path = "foo\0bar";
        Exception? exception =
            Record.Exception(() => FileSystem.Directory.CreateDirectory(path));

        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_ShouldCreateDirectoryInBasePath()
    {
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory("foo");
        bool exists = FileSystem.Directory.Exists("foo");

        exists.Should().BeTrue();
        result.FullName.Should().StartWith(BasePath);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_ShouldCreateParentDirectories(
        string directoryLevel1, string directoryLevel2, string directoryLevel3)
    {
        string path =
            FileSystem.Path.Combine(directoryLevel1, directoryLevel2, directoryLevel3);

        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.Name.Should().Be(directoryLevel3);
        result.Parent!.Name.Should().Be(directoryLevel2);
        result.Parent.Parent!.Name.Should().Be(directoryLevel1);
        result.Exists.Should().BeTrue();
        result.Parent.Exists.Should().BeTrue();
        result.Parent.Parent.Exists.Should().BeTrue();
#if NETFRAMEWORK
        result.ToString().Should().Be(directoryLevel3);
        result.Parent.ToString().Should().Be(result.Parent.Name);
        result.Parent.Parent.ToString().Should().Be(result.Parent.Parent.Name);
#else
        result.ToString().Should().Be(path);
        result.Parent.ToString().Should().Be(result.Parent.FullName);
        result.Parent.Parent.ToString().Should().Be(result.Parent.Parent.FullName);
#endif
    }

#if NETFRAMEWORK
    [Theory]
    [InlineData("/")]
    [InlineData("\\")]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_TrailingDirectorySeparator_ShouldNotBeTrimmed(
        string suffix)
    {
        string nameWithSuffix = "foobar" + suffix;
        string expectedName = nameWithSuffix;
        expectedName = expectedName.TrimEnd(' ');

        IFileSystem.IDirectoryInfo result =
            FileSystem.Directory.CreateDirectory(nameWithSuffix);

        result.ToString().Should().Be("");
        result.Name.Should().Be(expectedName.TrimEnd(
            FileSystem.Path.DirectorySeparatorChar,
            FileSystem.Path.AltDirectorySeparatorChar));
        result.FullName.Should().Be(Path.Combine(BasePath, expectedName
           .Replace(FileSystem.Path.AltDirectorySeparatorChar,
                FileSystem.Path.DirectorySeparatorChar)));
        FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_EmptyOrWhitespace_ShouldReturnEmptyString(
        string suffix)
    {
        string nameWithSuffix = "foobar" + suffix;
        string expectedName = nameWithSuffix;
        expectedName = expectedName.TrimEnd(' ');

        IFileSystem.IDirectoryInfo result =
            FileSystem.Directory.CreateDirectory(nameWithSuffix);

        result.ToString().Should().Be(expectedName);
        result.Name.Should().Be(expectedName.TrimEnd(
            FileSystem.Path.DirectorySeparatorChar,
            FileSystem.Path.AltDirectorySeparatorChar));
        result.FullName.Should().Be(Path.Combine(BasePath, expectedName
           .Replace(FileSystem.Path.AltDirectorySeparatorChar,
                FileSystem.Path.DirectorySeparatorChar)));
        FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
    }
#else
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("/")]
    [InlineData("\\")]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.CreateDirectory))]
    public void CreateDirectory_TrailingDirectorySeparator_ShouldNotBeTrimmed(
        string suffix)
    {
        string nameWithSuffix = "foobar" + suffix;
        string expectedName = nameWithSuffix;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            expectedName = expectedName.TrimEnd(' ');
        }
        else if (suffix == "\\")
        {
            //This case is only supported on Windows
            return;
        }

        IFileSystem.IDirectoryInfo result =
            FileSystem.Directory.CreateDirectory(nameWithSuffix);

        result.ToString().Should().Be(nameWithSuffix);
        result.Name.Should().Be(expectedName.TrimEnd(
            FileSystem.Path.DirectorySeparatorChar,
            FileSystem.Path.AltDirectorySeparatorChar));
        result.FullName.Should().Be(Path.Combine(BasePath, expectedName
           .Replace(FileSystem.Path.AltDirectorySeparatorChar,
                FileSystem.Path.DirectorySeparatorChar)));
        FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
    }
#endif
}