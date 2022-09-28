using System.IO;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
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
    public void CreateDirectory_ShouldSetCreationTimeUtc(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;

        FileSystem.Directory.CreateDirectory(path);

        DateTime result = FileSystem.Directory.GetCreationTimeUtc(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [AutoData]
    public void GetCreationTime_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToLocalTime();

        DateTime result = FileSystem.Directory.GetCreationTime(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void GetCreationTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToUniversalTime();

        DateTime result = FileSystem.Directory.GetCreationTimeUtc(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void GetLastAccessTime_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToLocalTime();

        DateTime result = FileSystem.Directory.GetLastAccessTime(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void GetLastAccessTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToUniversalTime();

        DateTime result = FileSystem.Directory.GetLastAccessTimeUtc(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void GetLastWriteTime_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToLocalTime();

        DateTime result = FileSystem.Directory.GetLastWriteTime(path);

        result.Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void GetLastWriteTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
    {
        DateTime expectedTime = NullTime.ToUniversalTime();

        DateTime result = FileSystem.Directory.GetLastWriteTimeUtc(path);

        result.Should().Be(expectedTime);
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

        FileSystem.Directory.CreateDirectory(path);

        DateTime result = FileSystem.Directory.GetLastAccessTime(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [AutoData]
    public void LastAccessTimeUtc_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;

        FileSystem.Directory.CreateDirectory(path);

        DateTime result = FileSystem.Directory.GetLastAccessTimeUtc(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [AutoData]
    public void LastWriteTime_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.Now;

        FileSystem.Directory.CreateDirectory(path);

        DateTime result = FileSystem.Directory.GetLastWriteTime(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.Now);
        result.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [AutoData]
    public void LastWriteTimeUtc_ShouldBeSet(string path)
    {
        DateTime start = TimeSystem.DateTime.UtcNow;

        FileSystem.Directory.CreateDirectory(path);

        DateTime result = FileSystem.Directory.GetLastWriteTimeUtc(path);
        result.Should().BeOnOrAfter(start.ApplySystemClockTolerance());
        result.Should().BeOnOrBefore(TimeSystem.DateTime.UtcNow);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [AutoData]
    public void SetCreationTime_PathNotFound_ShouldThrowFileNotFoundException(
        string path, DateTime creationTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.SetCreationTime(path, creationTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void SetCreationTime_ShouldChangeCreationTime(
        string path, DateTime creationTime)
    {
        creationTime = creationTime.ToLocalTime();
        DateTime expectedTime = creationTime.ToUniversalTime();
        FileSystem.Directory.CreateDirectory(path);

        FileSystem.Directory.SetCreationTime(path, creationTime);

        FileSystem.Directory.GetCreationTimeUtc(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void SetCreationTimeUtc_PathNotFound_ShouldThrowFileNotFoundException(
        string path, DateTime creationTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.SetCreationTimeUtc(path, creationTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void SetCreationTimeUtc_ShouldChangeCreationTime(
        string path, DateTime creationTime)
    {
        creationTime = creationTime.ToUniversalTime();
        DateTime expectedTime = creationTime.ToLocalTime();
        FileSystem.Directory.CreateDirectory(path);

        FileSystem.Directory.SetCreationTimeUtc(path, creationTime);

        FileSystem.Directory.GetCreationTime(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void SetLastAccessTime_PathNotFound_ShouldThrowFileNotFoundException(
        string path, DateTime lastAccessTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.SetLastAccessTime(path, lastAccessTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void SetLastAccessTime_ShouldChangeLastAccessTime(
        string path, DateTime lastAccessTime)
    {
        lastAccessTime = lastAccessTime.ToLocalTime();
        DateTime expectedTime = lastAccessTime.ToUniversalTime();
        FileSystem.Directory.CreateDirectory(path);

        FileSystem.Directory.SetLastAccessTime(path, lastAccessTime);

        FileSystem.Directory.GetLastAccessTimeUtc(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void SetLastAccessTimeUtc_PathNotFound_ShouldThrowFileNotFoundException(
        string path, DateTime lastAccessTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.SetLastAccessTimeUtc(path, lastAccessTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void SetLastAccessTimeUtc_ShouldChangeLastAccessTime(
        string path, DateTime lastAccessTime)
    {
        lastAccessTime = lastAccessTime.ToUniversalTime();
        DateTime expectedTime = lastAccessTime.ToLocalTime();
        FileSystem.Directory.CreateDirectory(path);

        FileSystem.Directory.SetLastAccessTimeUtc(path, lastAccessTime);

        FileSystem.Directory.GetLastAccessTime(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void SetLastWriteTime_PathNotFound_ShouldThrowFileNotFoundException(
        string path, DateTime lastWriteTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.SetLastWriteTime(path, lastWriteTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void SetLastWriteTime_ShouldChangeLastWriteTime(
        string path, DateTime lastWriteTime)
    {
        lastWriteTime = lastWriteTime.ToLocalTime();
        DateTime expectedTime = lastWriteTime.ToUniversalTime();
        FileSystem.Directory.CreateDirectory(path);

        FileSystem.Directory.SetLastWriteTime(path, lastWriteTime);

        FileSystem.Directory.GetLastWriteTimeUtc(path)
           .Should().Be(expectedTime);
    }

    [Theory]
    [AutoData]
    public void SetLastWriteTimeUtc_PathNotFound_ShouldThrowFileNotFoundException(
        string path, DateTime lastWriteTime)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.SetLastWriteTimeUtc(path, lastWriteTime);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    public void SetLastWriteTimeUtc_ShouldChangeLastWriteTime(
        string path, DateTime lastWriteTime)
    {
        lastWriteTime = lastWriteTime.ToUniversalTime();
        DateTime expectedTime = lastWriteTime.ToLocalTime();
        FileSystem.Directory.CreateDirectory(path);

        FileSystem.Directory.SetLastWriteTimeUtc(path, lastWriteTime);

        FileSystem.Directory.GetLastWriteTime(path)
           .Should().Be(expectedTime);
    }
}