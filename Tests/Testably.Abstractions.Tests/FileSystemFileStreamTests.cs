using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileStreamTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileStreamTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem,
        string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }

    #endregion

    [Theory]
    [AutoData]
    public void Read_ShouldCreateValidFileStream(string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents, Encoding.UTF8);
        FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open);
        using StreamReader sr = new(stream, Encoding.UTF8);
        string result = sr.ReadToEnd();

        result.Should().Be(contents);
    }

    [Theory]
    [AutoData]
    public void Write_ShouldCreateValidFileStream(string path, string contents)
    {
        FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.CreateNew);

        byte[] bytes = Encoding.UTF8.GetBytes(contents);
        stream.Write(bytes, 0, bytes.Length);

        stream.Dispose();

        string result = FileSystem.File.ReadAllText(path);

        result.Should().Be(contents);
    }

    [Theory(Skip = "TODO: File Access lock is not yet implemented for MockFileSystem")]
    [AutoData]
    public void Write_KeepOpen_ShouldCreateValidFileStream(string path, string contents)
    {
        FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.CreateNew);

        byte[] bytes = Encoding.UTF8.GetBytes(contents);
        stream.Write(bytes, 0, bytes.Length);

        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.ReadAllText(path);
        });

        exception.Should().BeOfType<IOException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void New_NullPath_ShouldThrowArgumentNullException(FileMode mode)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(null!, mode);
        });

        exception.Should().BeOfType<ArgumentNullException>()
           .Which.ParamName.Should().Be("path");
    }

    [Theory]
    [AutoData]
    public void New_EmptyPath_ShouldThrowArgumentException(FileMode mode)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(string.Empty, mode);
        });

#if NETFRAMEWORK
        exception.Should().BeOfType<ArgumentException>();
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.ParamName.Should().Be("path");
#endif
    }

    [Theory]
    [AutoData]
    public void New_AppendAccessWithReadWriteMode_ShouldThrowArgumentException(
        string path)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(path, FileMode.Append, FileAccess.ReadWrite);
        });

#if NETFRAMEWORK
        exception.Should().BeOfType<ArgumentException>();
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.ParamName.Should().Be("access");
#endif
        exception!.Message.Should()
           .Contain(FileMode.Append.ToString());
    }

    [Theory]
    [InlineAutoData(FileMode.Append)]
    [InlineAutoData(FileMode.Truncate)]
    [InlineAutoData(FileMode.Create)]
    [InlineAutoData(FileMode.CreateNew)]
    [InlineAutoData(FileMode.Append)]
    public void New_InvalidModeForReadAccess_ShouldThrowArgumentException(
        FileMode mode, string path)
    {
        FileAccess access = FileAccess.Read;
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(path, mode, access);
        });

#if NETFRAMEWORK
        exception.Should().BeOfType<ArgumentException>();
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.ParamName.Should().Be("access");
#endif
        exception!.Message.Should()
           .Contain(mode.ToString()).And
           .Contain(access.ToString());
    }

    [Theory]
    [InlineAutoData(FileMode.Open)]
    [InlineAutoData(FileMode.Truncate)]
    public void New_MissingFileWithIncorrectMode_ShouldThrowArgumentException(
        FileMode mode, string path)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(path, mode);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void New_ExistingFileWithCreateNewMode_ShouldThrowArgumentException(
        string path)
    {
        FileSystem.File.WriteAllText(path, "foo");
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(path, FileMode.CreateNew);
        });

        exception.Should().BeOfType<IOException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void New_SamePathAsExistingDirectory_ShouldThrowUnauthorizedAccessException(
        string path)
    {
        FileSystem.Directory.CreateDirectory(path);
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(path, FileMode.CreateNew);
        });

        exception.Should().BeOfType<UnauthorizedAccessException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }
}