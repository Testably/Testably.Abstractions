using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    /// <summary>
    ///     The default time returned by the file system if no time has been set.
    ///     <seealso href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times" />:
    ///     A file time is a 64-bit value that represents the number of 100-nanosecond intervals that have elapsed
    ///     since 12:00 A.M. January 1, 1601 Coordinated Universal Time (UTC).
    /// </summary>
    internal readonly DateTime NullTime = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    #endregion

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetCreationTime))]
    public void GetCreationTime_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToLocalTime();

        DateTime result = FileSystem.File.GetCreationTime(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetCreationTimeUtc))]
    public void GetCreationTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToUniversalTime();

        DateTime result = FileSystem.File.GetCreationTimeUtc(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetLastAccessTime))]
    public void GetLastAccessTime_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToLocalTime();

        DateTime result = FileSystem.File.GetLastAccessTime(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetLastAccessTimeUtc))]
    public void GetLastAccessTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToUniversalTime();

        DateTime result = FileSystem.File.GetLastAccessTimeUtc(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetLastWriteTime))]
    public void GetLastWriteTime_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToLocalTime();

        DateTime result = FileSystem.File.GetLastWriteTime(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetLastWriteTimeUtc))]
    public void GetLastWriteTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToUniversalTime();

        DateTime result = FileSystem.File.GetLastWriteTimeUtc(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetLastAccessTime))]
    public void LastAccessTime_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.Now;

        FileSystem.File.WriteAllText(path, null);

        DateTime result = FileSystem.File.GetLastAccessTime(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetLastAccessTimeUtc))]
    public void LastAccessTimeUtc_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;

        FileSystem.File.WriteAllText(path, null);

        DateTime result = FileSystem.File.GetLastAccessTimeUtc(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetLastWriteTime))]
    public void LastWriteTime_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.Now;

        FileSystem.File.WriteAllText(path, null);

        DateTime result = FileSystem.File.GetLastWriteTime(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.GetLastWriteTimeUtc))]
    public void LastWriteTimeUtc_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;

        FileSystem.File.WriteAllText(path, null);

        DateTime result = FileSystem.File.GetLastWriteTimeUtc(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetCreationTime))]
    public void SetCreationTime_PathNotFound_ShouldThrowCorrectException(
        string path, DateTime creationTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.SetCreationTime(path, creationTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetCreationTime))]
    public void SetCreationTime_ShouldChangeCreationTime(
        string path, DateTime creationTime)
    {
        Skip.IfNot(Test.RunsOnWindows,
            "Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

        creationTime = creationTime.ToLocalTime();
        DateTime expectedTime = creationTime.ToUniversalTime();
        FileSystem.File.WriteAllText(path, null);

        FileSystem.File.SetCreationTime(path, creationTime);

        FileSystem.File.GetCreationTimeUtc(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetCreationTimeUtc))]
    public void SetCreationTimeUtc_PathNotFound_ShouldThrowCorrectException(
        string path, DateTime creationTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.SetCreationTimeUtc(path, creationTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetCreationTimeUtc))]
    public void SetCreationTimeUtc_ShouldChangeCreationTime(
        string path, DateTime creationTime)
    {
        Skip.IfNot(Test.RunsOnWindows,
            "Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

        creationTime = creationTime.ToUniversalTime();
        DateTime expectedTime = creationTime.ToLocalTime();
        FileSystem.File.WriteAllText(path, null);

        FileSystem.File.SetCreationTimeUtc(path, creationTime);

        FileSystem.File.GetCreationTime(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetLastAccessTime))]
    public void SetLastAccessTime_PathNotFound_ShouldThrowCorrectException(
        string path, DateTime lastAccessTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.SetLastAccessTime(path, lastAccessTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetLastAccessTime))]
    public void SetLastAccessTime_ShouldChangeLastAccessTime(
        string path, DateTime lastAccessTime)
    {
        lastAccessTime = lastAccessTime.ToLocalTime();
        DateTime expectedTime = lastAccessTime.ToUniversalTime();
        FileSystem.File.WriteAllText(path, null);

        FileSystem.File.SetLastAccessTime(path, lastAccessTime);

        FileSystem.File.GetLastAccessTimeUtc(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetLastAccessTimeUtc))]
    public void SetLastAccessTimeUtc_PathNotFound_ShouldThrowCorrectException(
        string path, DateTime lastAccessTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.SetLastAccessTimeUtc(path, lastAccessTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetLastAccessTimeUtc))]
    public void SetLastAccessTimeUtc_ShouldChangeLastAccessTime(
        string path, DateTime lastAccessTime)
    {
        lastAccessTime = lastAccessTime.ToUniversalTime();
        DateTime expectedTime = lastAccessTime.ToLocalTime();
        FileSystem.File.WriteAllText(path, null);

        FileSystem.File.SetLastAccessTimeUtc(path, lastAccessTime);

        FileSystem.File.GetLastAccessTime(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetLastWriteTime))]
    public void SetLastWriteTime_PathNotFound_ShouldThrowCorrectException(
        string path, DateTime lastWriteTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.SetLastWriteTime(path, lastWriteTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetLastWriteTime))]
    public void SetLastWriteTime_ShouldChangeLastWriteTime(
        string path, DateTime lastWriteTime)
    {
        lastWriteTime = lastWriteTime.ToLocalTime();
        DateTime expectedTime = lastWriteTime.ToUniversalTime();
        FileSystem.File.WriteAllText(path, null);

        FileSystem.File.SetLastWriteTime(path, lastWriteTime);

        FileSystem.File.GetLastWriteTimeUtc(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetLastWriteTimeUtc))]
    public void SetLastWriteTimeUtc_PathNotFound_ShouldThrowCorrectException(
        string path, DateTime lastWriteTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.SetLastWriteTimeUtc(path, lastWriteTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.SetLastWriteTimeUtc))]
    public void SetLastWriteTimeUtc_ShouldChangeLastWriteTime(
        string path, DateTime lastWriteTime)
    {
        lastWriteTime = lastWriteTime.ToUniversalTime();
        DateTime expectedTime = lastWriteTime.ToLocalTime();
        FileSystem.File.WriteAllText(path, null);

        FileSystem.File.SetLastWriteTimeUtc(path, lastWriteTime);

        FileSystem.File.GetLastWriteTime(path)
           .Should().Be(expectedTime);
    }
}