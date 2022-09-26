using System;

namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
    /// <summary>
    ///     Initializes a directory in the <see cref="IFileSystem" /> with test data.
    /// </summary>
    public interface IFileSystemDirectoryInitializer<out TFileSystem>
        : IFileSystemInitializer<TFileSystem>
        where TFileSystem : IFileSystem
    {
        /// <summary>
        ///     The directory to initialize.
        /// </summary>
        public IFileSystem.IDirectoryInfo Directory { get; }

        /// <summary>
        ///     Initializes the subdirectory in the <see cref="IFileSystem" /> with test data.
        /// </summary>
        public IFileSystemDirectoryInitializer<TFileSystem> Initialized(
            Action<IFileSystemInitializer<TFileSystem>> subdirectoryInitializer);
    }

    /// <summary>
    ///     Initializes a file in the <see cref="IFileSystem" /> with test data.
    /// </summary>
    public interface IFileSystemFileInitializer<out TFileSystem>
        : IFileSystemInitializer<TFileSystem>
        where TFileSystem : IFileSystem
    {
        /// <summary>
        ///     The file to initialize.
        /// </summary>
        public IFileSystem.IFileInfo File { get; }

        /// <summary>
        ///     Manipulates the <see cref="File" /> in the <see cref="IFileSystem" /> with test data.
        /// </summary>
        public IFileSystemFileInitializer<TFileSystem> Which(
            Action<IFileManipulator> fileManipulation);
    }

    /// <summary>
    ///     Manipulates the <see cref="File" /> in the <see cref="IFileSystem" /> with test data.
    /// </summary>
    public interface IFileManipulator : IFileSystem.IFileSystemExtensionPoint
    {
        /// <summary>
        ///     The file to initialize.
        /// </summary>
        public IFileSystem.IFileInfo File { get; }

        /// <summary>
        ///     Sets the contents of the <see cref="File" /> to <paramref name="contents" />.
        /// </summary>
        IFileManipulator HasStringContent(string contents);

        /// <summary>
        ///     Sets the contents of the <see cref="File" /> to <paramref name="bytes" />.
        /// </summary>
        IFileManipulator HasBytesContent(byte[] bytes);
    }

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