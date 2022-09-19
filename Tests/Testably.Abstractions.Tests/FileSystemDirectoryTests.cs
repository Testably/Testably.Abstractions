using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests
{
    #region Test Setup

    public IFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }
    public string BasePath { get; }

    protected FileSystemDirectoryTests(IFileSystem fileSystem, ITimeSystem timeSystem,
                                       string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }

    #endregion

    [Fact]
    public void Create_Empty_ShouldThrowArgumentException()
    {
        Exception? exception =
            Record.Exception(() => FileSystem.DirectoryInfo.New(string.Empty));

        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.ParamName.Should().Be("path");
        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.Message.Should()
           .Be("The path is empty. (Parameter 'path')");
    }

    [Fact]
    public void Create_IllegalCharacters_ShouldThrowArgumentException()
    {
        foreach (char c in FileSystem.Path.GetInvalidPathChars().Where(c => c != '\0'))
        {
            string path = "foo" + c + "bar";
            string expectedMessage =
                $"The filename, directory name, or volume label syntax is incorrect. : '{Path.Combine(BasePath, path)}'";
            Exception? exception =
                Record.Exception(() => FileSystem.DirectoryInfo.New(path).Create());

            exception.Should().BeAssignableTo<IOException>()
               .Which.Message.Should().Be(expectedMessage);
        }
    }

    [Fact]
    public void Create_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception =
            Record.Exception(() => FileSystem.DirectoryInfo.New(null!));

        exception.Should().BeAssignableTo<ArgumentNullException>().Which.ParamName
           .Should().Be("path");
    }

    [Fact]
    public void Create_NullCharacter_ShouldThrowArgumentException()
    {
        string path = "foo\0bar";
        string expectedMessage =
            "Illegal characters in path. (Parameter 'path')";
        Exception? exception =
            Record.Exception(() => FileSystem.DirectoryInfo.New(path).Create());

        exception.Should().BeAssignableTo<ArgumentException>()
           .Which.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldCreateInBasePath()
    {
        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New("foo");
        result.Create();
        bool exists = FileSystem.Directory.Exists("foo");

        exists.Should().BeTrue();
        result.FullName.Should().StartWith(BasePath);
    }

    [Theory]
    [AutoData]
    public void Create_ShouldCreateParentDirectories(
        string directoryLevel1, string directoryLevel2, string directoryLevel3)
    {
        string path =
            FileSystem.Path.Combine(directoryLevel1, directoryLevel2, directoryLevel3);

        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);
        result.Create();

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
    [InlineData("/")]
    [InlineData("\\")]
    public void Create_TrailingDirectorySeparator_ShouldNotBeTrimmed(
        string suffix)
    {
        string nameWithSuffix = "foobar" + suffix;
        string expectedName = nameWithSuffix;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            expectedName = expectedName.TrimEnd(' ');
        }
        else if (suffix == "\\" || suffix == " ")
        {
            //TODO: This case is only supported on Windows
            return;
        }

        IFileSystem.IDirectoryInfo result =
            FileSystem.DirectoryInfo.New(nameWithSuffix);
        result.Create();

        result.ToString().Should().Be(nameWithSuffix);
        result.Name.Should().Be(expectedName.TrimEnd(
            FileSystem.Path.DirectorySeparatorChar,
            FileSystem.Path.AltDirectorySeparatorChar));
        result.FullName.Should().Be(Path.Combine(BasePath, expectedName
           .Replace(FileSystem.Path.AltDirectorySeparatorChar,
                FileSystem.Path.DirectorySeparatorChar)));
        FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
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
    [AutoData]
    public void CreateDirectory_ShouldCreateParentDirectories(
        string directoryLevel1, string directoryLevel2, string directoryLevel3)
    {
        string path =
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

    [Theory]
    [AutoData]
    public void CreationTime_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.Now;
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.CreationTime.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.CreationTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.CreationTime.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [AutoData]
    public void CreationTimeUtc_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.CreationTimeUtc.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.CreationTimeUtc.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.CreationTimeUtc.Kind.Should().Be(DateTimeKind.Utc);
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

    [Theory]
    [AutoData]
    public void LastAccessTime_CreateSubDirectory_ShouldUpdateLastAccessAndLastWriteTime(
        string path, string subPath)
    {
        DateTime start = TimeSystem.DateTime.Now;
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);
        TimeSystem.Thread.Sleep(100);
        DateTime sleepTime = TimeSystem.DateTime.Now;
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subPath));

        result.CreationTime.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.CreationTime.Should().BeBefore(sleepTime);
        // Last Access Time is only updated on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            result.LastAccessTime.Should()
               .BeOnOrAfter(sleepTime.ApplySystemClockTolerance());
            result.LastAccessTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        }
        else
        {
            result.LastAccessTime.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
            result.LastAccessTime.Should().BeBefore(sleepTime);
        }

        result.LastWriteTime.Should().BeOnOrAfter(sleepTime.ApplySystemClockTolerance());
        result.LastWriteTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
    }

    [Theory]
    [AutoData]
    public void LastAccessTime_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.Now;
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.LastAccessTime.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.LastAccessTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.LastAccessTime.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [AutoData]
    public void LastAccessTimeUtc_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.LastAccessTimeUtc.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.LastAccessTimeUtc.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.LastAccessTimeUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [AutoData]
    public void LastWriteTime_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.Now;
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.LastWriteTime.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.LastWriteTime.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.LastWriteTime.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [AutoData]
    public void LastWriteTimeUtc_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.LastWriteTimeUtc.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.LastWriteTimeUtc.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.LastWriteTimeUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [AutoData]
    public void Root_ShouldExist(string path)
    {
        string expectedRoot = "".PrefixRoot();
        IFileSystem.IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);

        result.Root.Exists.Should().BeTrue();
        result.Root.FullName.Should().Be(expectedRoot);
    }
}