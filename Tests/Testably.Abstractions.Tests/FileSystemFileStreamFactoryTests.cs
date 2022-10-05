using System.IO;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileStreamFactoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public abstract string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileStreamFactoryTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;

        Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
    }

    #endregion

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStreamFactory(nameof(IFileSystem.IFileStreamFactory.New))]
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

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStreamFactory(nameof(IFileSystem.IFileStreamFactory.New))]
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

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStreamFactory(nameof(IFileSystem.IFileStreamFactory.New))]
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

    [SkippableTheory]
    [InlineAutoData(FileMode.Append)]
    [InlineAutoData(FileMode.Truncate)]
    [InlineAutoData(FileMode.Create)]
    [InlineAutoData(FileMode.CreateNew)]
    [InlineAutoData(FileMode.Append)]
    [FileSystemTests.FileStreamFactory(nameof(IFileSystem.IFileStreamFactory.New))]
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

    [SkippableTheory]
    [InlineAutoData(FileMode.Open)]
    [InlineAutoData(FileMode.Truncate)]
    [FileSystemTests.FileStreamFactory(nameof(IFileSystem.IFileStreamFactory.New))]
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

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStreamFactory(nameof(IFileSystem.IFileStreamFactory.New))]
    public void New_NullPath_ShouldThrowArgumentNullException(FileMode mode)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(null!, mode);
        });

        exception.Should().BeOfType<ArgumentNullException>()
           .Which.ParamName.Should().Be("path");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStreamFactory(nameof(IFileSystem.IFileStreamFactory.New))]
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
}