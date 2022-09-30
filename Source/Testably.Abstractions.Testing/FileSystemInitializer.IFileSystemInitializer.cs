﻿namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
    /// <summary>
    ///     Initializes the <see cref="IFileSystem" /> with test data.
    /// </summary>
    public interface IFileSystemInitializer<out TFileSystem>
        where TFileSystem : IFileSystem
    {
        /// <summary>
        /// </summary>
        TFileSystem FileSystem { get; }

        /// <summary>
        ///     Gives access to the created files or directories in the order of the initialization.
        /// </summary>
        IFileSystem.IFileSystemInfo this[int index]
        {
            get;
        }

        /// <summary>
        ///     Initializes the <see cref="IFileSystem" /> with a randomly named file.
        /// </summary>
        /// <param name="extension">(optional) If specified, uses the given extension for the file.</param>
        IFileSystemFileInitializer<TFileSystem> WithAFile(string? extension = null);

        /// <summary>
        ///     Initializes the <see cref="IFileSystem" /> with a randomly named subdirectory.
        /// </summary>
        /// <returns></returns>
        IFileSystemDirectoryInitializer<TFileSystem> WithASubdirectory();

        /// <summary>
        ///     Initializes the <see cref="IFileSystem" /> with a file with the given <paramref name="fileName" />.
        /// </summary>
        IFileSystemFileInitializer<TFileSystem> WithFile(string fileName);

        /// <summary>
        ///     Initializes the <see cref="IFileSystem" /> with a subdirectory with the given <paramref name="directoryName" />.
        /// </summary>
        IFileSystemDirectoryInitializer<TFileSystem> WithSubdirectory(
            string directoryName);
    }
}