using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    /// <summary>
    ///     The default time returned by the file system if no time has been set.
    ///     <seealso href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times" />:
    ///     A file time is a 64-bit value that represents the number of 100-nanosecond intervals that have elapsed
    ///     since 12:00 A.M. January 1, 1601 Coordinated Universal Time (UTC).
    /// </summary>
    internal readonly DateTime NullTime = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
}