using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Initializes the <see cref="IFileSystem" /> with test data.
/// </summary>
public static partial class FileSystemInitializer
{
    /// <summary>
    ///     Initializes the <see cref="IFileSystem" /> in the working directory with test data.
    /// </summary>
    public static IFileSystemInitializer<TFileSystem> Initialize<TFileSystem>(
        this TFileSystem fileSystem)
        where TFileSystem : IFileSystem
        => fileSystem.InitializeIn(".");

    /// <summary>
    ///     Initializes the <see cref="IFileSystem" /> in the <paramref name="basePath" /> with test data.
    /// </summary>
    public static IFileSystemInitializer<TFileSystem> InitializeIn<TFileSystem>(
        this TFileSystem fileSystem,
        string basePath)
        where TFileSystem : IFileSystem
    {
        fileSystem.Directory.CreateDirectory(basePath);
        return new Initializer<TFileSystem>(fileSystem, basePath);
    }

    /// <summary>
    ///     Initializes the <see cref="IFileSystem.IDirectory.GetCurrentDirectory()" /> to a new temporary directory.
    ///     <para />
    ///     Returns a <see cref="IDisposable" /> class that will force delete all content in this temporary directory.
    /// </summary>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="logger">(optional) A callback to log the cleanup process.</param>
    /// <returns>A <see cref="IDisposable" /> class that will force delete all content in the temporary directory.</returns>
    public static IDirectoryCleaner InitializeCurrentDirectoryInEmptyTemporaryDirectory(
        this IFileSystem fileSystem, Action<string>? logger = null)
    {
        return new TemporaryDirectoryCleaner(fileSystem, logger);
    }

    public interface IDirectoryCleaner : IDisposable
    {
        string BasePath { get; }
    }

    private sealed class TemporaryDirectoryCleaner : IDirectoryCleaner
    {
        private readonly IFileSystem _fileSystem;
        private readonly Action<string>? _logger;
        public string BasePath { get; }

        public TemporaryDirectoryCleaner(IFileSystem fileSystem, Action<string>? logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            BasePath = InitializeBasePath();
        }

        private string InitializeBasePath()
        {
            string basePath;

            do
            {
                basePath = _fileSystem.Path.Combine(
                    _fileSystem.Path.GetTempPath(),
                    _fileSystem.Path.GetFileNameWithoutExtension(_fileSystem.Path
                       .GetRandomFileName()));
            } while (_fileSystem.Directory.Exists(basePath));

            _fileSystem.Directory.CreateDirectory(basePath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                basePath = "/private" + basePath;
            }

            _logger?.Invoke($"Use '{basePath}' as current directory.");
            _fileSystem.Directory.SetCurrentDirectory(basePath);
            return basePath;
        }

        /// <summary>
        ///     Force deletes the directory at the given <paramref name="path" />.<br />
        ///     Removes the <see cref="FileAttributes.ReadOnly" /> flag, if necessary.
        ///     <para />
        ///     If <paramref name="recursive" /> is set (default <c>true</c>), the sub directories are force deleted as well.
        /// </summary>
        private void ForceDeleteDirectory(string path, bool recursive = true)
        {
            if (!_fileSystem.Directory.Exists(path))
            {
                return;
            }

            IFileSystem.IDirectoryInfo directory = _fileSystem.DirectoryInfo.New(path);
            directory.Attributes = FileAttributes.Normal;

            foreach (IFileSystem.IFileInfo info in directory.EnumerateFiles("*",
                SearchOption.TopDirectoryOnly))
            {
                info.Attributes = FileAttributes.Normal;
                info.Delete();
            }

            if (recursive)
            {
                foreach (IFileSystem.IDirectoryInfo info in
                    directory.EnumerateDirectories("*",
                        SearchOption.TopDirectoryOnly))
                {
                    ForceDeleteDirectory(info.FullName, recursive);
                }
            }

            _fileSystem.Directory.Delete(path);
        }

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose()
        {
            try
            {
                // It is important to reset the current directory, as otherwise deleting the BasePath
                // results in a IOException, because the process cannot access the file.
                Directory.SetCurrentDirectory(Path.GetTempPath());

                _logger?.Invoke($"Cleaning up '{BasePath}'...");
                for (int i = 10; i >= 0; i--)
                {
                    try
                    {
                        ForceDeleteDirectory(BasePath);
                    }
                    catch (Exception)
                    {
                        if (i == 0)
                        {
                            throw;
                        }

                        _logger?.Invoke(
                            $"  Force delete failed! Retry again {i} times in 100ms...");
                        Thread.Sleep(100);
                    }
                }

                _logger?.Invoke($"Cleaned up '{BasePath}'.");
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Could not clean up '{BasePath}' because: {ex}");
            }
        }
    }
}