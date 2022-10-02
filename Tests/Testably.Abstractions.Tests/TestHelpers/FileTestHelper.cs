using System.IO;

namespace Testably.Abstractions.Tests.TestHelpers;

public static class FileTestHelper
{
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