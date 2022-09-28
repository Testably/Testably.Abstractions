using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Testably.Abstractions.Tests.TestHelpers.Attributes;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
    /// <summary>
    ///     Force deletes the directory at the given <paramref name="path" />.<br />
    ///     Removes the <see cref="FileAttributes.ReadOnly" /> flag, if necessary.
    ///     <para />
    ///     If <paramref name="recursive" /> is set (default <c>true</c>), the sub directories are force deleted as well.
    /// </summary>
    private static void ForceDeleteDirectory(string path, bool recursive = true)
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

    private static void TryCleanup(string basePath, ITestOutputHelper testOutputHelper)
    {
        try
        {
            // It is important to reset the current directory, as otherwise deleting the BasePath
            // results in a IOException, because the process cannot access the file.
            Directory.SetCurrentDirectory(Path.GetTempPath());

            testOutputHelper.WriteLine($"Cleaning up '{basePath}'...");
            for (int i = 10; i >= 0; i--)
            {
                try
                {
                    ForceDeleteDirectory(basePath);
                }
                catch (Exception)
                {
                    if (i == 0)
                    {
                        throw;
                    }

                    testOutputHelper.WriteLine(
                        $"  Force delete failed! Retry again {i} times in 100ms...");
                    Thread.Sleep(100);
                }
            }

            testOutputHelper.WriteLine($"Cleaned up '{basePath}'.");
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine(
                $"Could not clean up '{basePath}' because: {ex}");
        }
    }

    /// <summary>
    ///     Creates a new and empty directory in the temporary path.
    /// </summary>
    private static string UseBasePath(ITestOutputHelper testOutputHelper)
    {
        string basePath;

        do
        {
            basePath = Path.Combine(
                Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        } while (Directory.Exists(basePath));

        Directory.CreateDirectory(basePath);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            basePath = "/private" + basePath;
        }

        testOutputHelper.WriteLine($"Use '{basePath}' as current directory.");
        Directory.SetCurrentDirectory(basePath);
        return basePath;
    }

    // ReSharper disable once UnusedMember.Global
    [Collection(nameof(RealFileSystemTestAttribute))]
    public sealed class Tests : FileSystemTests<FileSystem>
    {
        public Tests() : base(new FileSystem())
        {
        }
    }
}