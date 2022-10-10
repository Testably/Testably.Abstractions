using System.IO;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileInfoFactoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public abstract string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileInfoFactoryTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;

        Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
    }

    #endregion

    [SkippableFact]
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.New))]
    public void New_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.FileInfo.New(null!);
        });

        exception.Should().BeOfType<ArgumentNullException>();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.New))]
    public void New_ShouldCreateNewFileInfoFromPath(string path)
    {
        IFileSystem.IFileInfo result = FileSystem.FileInfo.New(path);

        result.ToString().Should().Be(path);
        result.Exists.Should().BeFalse();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.New))]
    public void New_ShouldOpenWithExistingContent(string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using StreamReader streamReader = new(sut.OpenRead());
        string result = streamReader.ReadToEnd();
        result.Should().Be(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.New))]
    public void New_ShouldSetLength(string path, byte[] bytes)
    {
        FileSystem.File.WriteAllBytes(path, bytes);

        //TODO: Reactivate this test
        //var stream = FileSystem.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Write);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        long result = sut.Length;

        //stream.Dispose();

        result.Should().Be(bytes.Length);
    }

    [SkippableFact]
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.Wrap))]
    public void Wrap_Null_ShouldReturnNull()
    {
        IFileSystem.IFileInfo? result = FileSystem.FileInfo.Wrap(null);

        result.Should().BeNull();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.Wrap))]
    public void Wrap_ShouldWrapFromFileInfo(string path)
    {
        FileInfo fileInfo = new(path);

        IFileSystem.IFileInfo result = FileSystem.FileInfo.Wrap(fileInfo);

        result.FullName.Should().Be(fileInfo.FullName);
        result.Exists.Should().Be(fileInfo.Exists);
    }
}