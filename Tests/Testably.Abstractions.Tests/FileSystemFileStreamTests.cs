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

    [Theory(Skip="TODO: File Access lock is not yet implemented for MockFileSystem")]
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
    public void New_EmptyPath_ShouldThrowArgumentNullException(FileMode mode)
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
    public void New_InvalidAccess_ShouldThrowArgumentNullException(
        string path, FileMode mode)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(path, FileMode.Append, FileAccess.Read);
        });

#if NETFRAMEWORK
        exception.Should().BeOfType<ArgumentException>();
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.ParamName.Should().Be("access");
#endif
    }
}