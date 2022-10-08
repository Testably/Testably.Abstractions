using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public abstract string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemDirectoryInfoTests(
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
    [FileSystemTests.DirectoryInfo("MissingFile")]
    public void MissingFile_Attributes_ShouldAlwaysBeNegativeOne(
        FileAttributes fileAttributes)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
        sut.Attributes.Should().Be((FileAttributes)(-1));
        sut.Attributes = fileAttributes;
        sut.Attributes.Should().Be((FileAttributes)(-1));
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo("MissingFile")]
    public void MissingFile_CreationTime_ShouldAlwaysBeNullTime(DateTime creationTime)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
        sut.CreationTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
        sut.CreationTime = creationTime;
        sut.CreationTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo("MissingFile")]
    public void MissingFile_CreationTimeUtc_ShouldAlwaysBeNullTime(
        DateTime creationTimeUtc)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
        sut.CreationTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
        sut.CreationTimeUtc = creationTimeUtc;
        sut.CreationTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo("MissingFile")]
    public void MissingFile_LastAccessTime_ShouldAlwaysBeNullTime(DateTime lastAccessTime)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
        sut.LastAccessTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
        sut.LastAccessTime = lastAccessTime;
        sut.LastAccessTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo("MissingFile")]
    public void MissingFile_LastAccessTimeUtc_ShouldAlwaysBeNullTime(
        DateTime lastAccessTimeUtc)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
        sut.LastAccessTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
        sut.LastAccessTimeUtc = lastAccessTimeUtc;
        sut.LastAccessTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo("MissingFile")]
    public void MissingFile_LastWriteTime_ShouldAlwaysBeNullTime(DateTime lastWriteTime)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
        sut.LastWriteTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
        sut.LastWriteTime = lastWriteTime;
        sut.LastWriteTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo("MissingFile")]
    public void MissingFile_LastWriteTimeUtc_ShouldAlwaysBeNullTime(
        DateTime lastWriteTimeUtc)
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
        sut.LastWriteTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
        sut.LastWriteTimeUtc = lastWriteTimeUtc;
        sut.LastWriteTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Parent))]
    public void Parent_ArbitraryPaths_ShouldNotBeNull(string path1,
                                                      string path2,
                                                      string path3)
    {
        string path = FileSystem.Path.Combine(path1, path2, path3);

        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

        sut.Parent.Should().NotBeNull();
        sut.Parent!.Exists.Should().BeFalse();
        sut.Parent.Parent.Should().NotBeNull();
        sut.Parent.Parent!.Exists.Should().BeFalse();
    }

    [SkippableFact]
    [AutoData]
    [FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Parent))]
    public void Parent_Root_ShouldBeNull()
    {
        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("".PrefixRoot());

        sut.Parent.Should().BeNull();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Root))]
    public void Root_ShouldExist(string path)
    {
        string expectedRoot = string.Empty.PrefixRoot();
        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

        result.Root.Exists.Should().BeTrue();
        result.Root.FullName.Should().Be(expectedRoot);
    }
}