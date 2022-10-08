using System.IO;

namespace Testably.Abstractions.Tests.TestHelpers;

public static class FileTestHelper
{
    #region Test Setup

    /// <summary>
    ///     The default time returned by the file system if no time has been set.
    ///     <seealso href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times" />:
    ///     A file time is a 64-bit value that represents the number of 100-nanosecond intervals that have elapsed
    ///     since 12:00 A.M. January 1, 1601 Coordinated Universal Time (UTC).
    /// </summary>
    internal static readonly DateTime NullTime = new(1601, 1, 1, 0, 0, 0,
        DateTimeKind.Utc);

    #endregion

    public static FileAccess CheckFileAccess(FileSystemStream stream)
    {
        FileAccess fileAccess = 0;
        if (stream.CanRead)
        {
            fileAccess |= FileAccess.Read;
        }

        if (stream.CanWrite)
        {
            fileAccess |= FileAccess.Write;
        }

        return fileAccess;
    }

    public static FileShare CheckFileShare(IFileSystem fileSystem, string path)
    {
        FileShare fileShare = FileShare.None;
        Exception? exception = Record.Exception(() =>
        {
            fileSystem.File.Open(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite)
               .Dispose();
        });
        if (exception == null)
        {
            fileShare |= FileShare.Read;
        }

        exception = Record.Exception(() =>
        {
            fileSystem.File.Open(
                    path,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.ReadWrite)
               .Dispose();
        });
        if (exception == null)
        {
            fileShare |= FileShare.Write;
        }

        return fileShare;
    }
}