using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Tests.TestHelpers.Attributes;

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
    [InlineAutoData(FileAccess.Read, FileShare.Read,
        FileAccess.ReadWrite, FileShare.Read)]
    [InlineAutoData(FileAccess.ReadWrite, FileShare.Read,
        FileAccess.Read, FileShare.Read)]
    [InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite,
        FileAccess.ReadWrite, FileShare.Read)]
    [InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite,
        FileAccess.ReadWrite, FileShare.Write)]
    [InlineAutoData(FileAccess.Read, FileShare.Read,
        FileAccess.ReadWrite, FileShare.ReadWrite)]
    public void FileAccess_ConcurrentAccessWithInvalidScenarios_ShouldThrowIOException(
        FileAccess access1, FileShare share1,
        FileAccess access2, FileShare share2,
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.FileStream.New(path, FileMode.Open,
                access1, share1);
            _ = FileSystem.FileStream.New(path, FileMode.Open,
                access2, share2);
        });

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            exception.Should().BeOfType<IOException>($"Access {access1}, Share {share1} of file 1 is incompatible with Access {access2}, Share {share2} of file 2")
               .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
        }
        else
        {
            exception.Should().BeNull();
        }
    }

    [Theory]
    [InlineAutoData(FileAccess.Read, FileShare.Read, FileAccess.Read, FileShare.Read)]
    [InlineAutoData(FileAccess.Read, FileShare.ReadWrite, FileAccess.ReadWrite,
        FileShare.Read)]
    public void FileAccess_ConcurrentReadAccessWithValidScenarios_ShouldNotThrowException(
        FileAccess access1, FileShare share1,
        FileAccess access2, FileShare share2,
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        FileSystemStream stream1 = FileSystem.FileStream.New(path, FileMode.Open,
            access1, share1);
        FileSystemStream stream2 = FileSystem.FileStream.New(path, FileMode.Open,
            access2, share2);

        using StreamReader sr1 = new(stream1, Encoding.UTF8);
        using StreamReader sr2 = new(stream2, Encoding.UTF8);
        string result1 = sr1.ReadToEnd();
        string result2 = sr2.ReadToEnd();

        result1.Should().Be(contents);
        result2.Should().Be(contents);
    }

    [Theory]
    [InlineAutoData(FileAccess.Write, FileShare.Write, FileAccess.Write, FileShare.Write)]
    [InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite, FileAccess.ReadWrite,
        FileShare.ReadWrite)]
    public void
        FileAccess_ConcurrentWriteAccessWithValidScenarios_ShouldNotThrowException(
            FileAccess access1, FileShare share1,
            FileAccess access2, FileShare share2,
            string path, string contents1, string contents2)
    {
        FileSystem.File.WriteAllText(path, null);

        FileSystemStream stream1 = FileSystem.FileStream.New(path, FileMode.Open,
            access1, share1);
        FileSystemStream stream2 = FileSystem.FileStream.New(path, FileMode.Open,
            access2, share2);

        byte[] bytes1 = Encoding.UTF8.GetBytes(contents1);
        stream1.Write(bytes1, 0, bytes1.Length);
        stream1.Flush();
        byte[] bytes2 = Encoding.UTF8.GetBytes(contents2);
        stream2.Write(bytes2, 0, bytes2.Length);
        stream2.Flush();

        stream1.Dispose();
        stream2.Dispose();
        string result = FileSystem.File.ReadAllText(path);

        result.Should().Be(contents2);
    }

    [Theory]
    [AutoData]
    public void FileAccess_ReadAfterFirstAppend_ShouldContainBothContents(
        string path, string contents1, string contents2)
    {
        FileSystem.File.WriteAllText(path, null);

        FileSystemStream stream1 = FileSystem.FileStream.New(path, FileMode.Append,
            FileAccess.Write, FileShare.Write);

        byte[] bytes1 = Encoding.UTF8.GetBytes(contents1);
        stream1.Write(bytes1, 0, bytes1.Length);
        stream1.Flush();

        FileSystemStream stream2 = FileSystem.FileStream.New(path, FileMode.Append,
            FileAccess.Write, FileShare.Write);
        byte[] bytes2 = Encoding.UTF8.GetBytes(contents2);
        stream2.Write(bytes2, 0, bytes2.Length);
        stream2.Flush();

        stream1.Dispose();
        stream2.Dispose();
        string result = FileSystem.File.ReadAllText(path);
        result.Should().Be(contents1 + contents2);
    }

    [Theory]
    [AutoData]
    public void FileAccess_ReadBeforeFirstAppend_ShouldOnlyContainSecondContent(
        string path, string contents1, string contents2)
    {
        FileSystem.File.WriteAllText(path, null);

        FileSystemStream stream1 = FileSystem.FileStream.New(path, FileMode.Append,
            FileAccess.Write, FileShare.Write);
        FileSystemStream stream2 = FileSystem.FileStream.New(path, FileMode.Append,
            FileAccess.Write, FileShare.Write);

        byte[] bytes1 = Encoding.UTF8.GetBytes(contents1);
        stream1.Write(bytes1, 0, bytes1.Length);
        stream1.Flush();
        byte[] bytes2 = Encoding.UTF8.GetBytes(contents2);
        stream2.Write(bytes2, 0, bytes2.Length);
        stream2.Flush();

        stream1.Dispose();
        stream2.Dispose();
        string result = FileSystem.File.ReadAllText(path);

        result.Should().Be(contents2);
    }

    [Theory]
    [AutoData]
    public void FileAccess_ReadWhileWriteLockActive_ShouldThrowIOException(
        string path, string contents)
    {
        FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Create);

        byte[] bytes = Encoding.UTF8.GetBytes(contents);
        stream.Write(bytes, 0, bytes.Length);

        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.ReadAllText(path);
        });
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            exception.Should().BeOfType<IOException>()
               .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
        }
        else
        {
            exception.Should().BeNull();
        }
    }

    [Theory]
    [AutoData]
    public void MultipleParallelReads_ShouldBeAllowed(string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);
        ConcurrentBag<string> results = new();

        ParallelLoopResult wait = Parallel.For(0, 100, _ =>
        {
            FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
                FileAccess.Read, FileShare.Read);
            using StreamReader sr = new(stream, Encoding.UTF8);
            results.Add(sr.ReadToEnd());
        });

        while (!wait.IsCompleted)
        {
            Thread.Sleep(10);
        }

        results.Should().HaveCount(100);
        results.Should().AllBeEquivalentTo(contents);
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
    public void New_SamePathAsExistingDirectory_ShouldThrowException(
        string path)
    {
        FileSystem.Directory.CreateDirectory(path);
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(path, FileMode.CreateNew);
        });

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            exception.Should().BeOfType<UnauthorizedAccessException>()
               .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
        }
        else
        {
            exception.Should().BeOfType<IOException>()
               .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
        }
    }

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
}