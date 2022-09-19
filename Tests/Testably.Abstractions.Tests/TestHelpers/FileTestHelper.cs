using System.IO;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers;

public static class FileTestHelper
{
    /// <summary>
    ///     Creates a new and empty directory in the temporary path.
    /// </summary>
    public static string CreateEmptyTemporaryDirectory()
    {
        string tmpDirectory;

        do
        {
            tmpDirectory = Path.Combine(
                Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        } while (Directory.Exists(tmpDirectory));

        Directory.CreateDirectory(tmpDirectory);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "/private" + tmpDirectory;
        }

        return tmpDirectory;
    }

    /// <summary>
    ///     Force deletes the directory at the given <paramref name="path" />.<br />
    ///     Removes the <see cref="FileAttributes.ReadOnly" /> flag, if necessary.
    ///     <para />
    ///     If <paramref name="recursive" /> is set (default <c>true</c>), the sub directories are force deleted as well.
    /// </summary>
    public static void ForceDeleteDirectory(string path, bool recursive = true)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        DirectoryInfo directory = new(path) { Attributes = FileAttributes.Normal };

        foreach (FileInfo info in directory.EnumerateFiles("*",
            SearchOption.TopDirectoryOnly))
        {
            info.Attributes = FileAttributes.Normal;
            info.Delete();
        }

        if (recursive)
        {
            foreach (DirectoryInfo info in directory.EnumerateDirectories("*",
                SearchOption.TopDirectoryOnly))
            {
                ForceDeleteDirectory(info.FullName, recursive);
            }
        }

        Directory.Delete(path);
    }
}