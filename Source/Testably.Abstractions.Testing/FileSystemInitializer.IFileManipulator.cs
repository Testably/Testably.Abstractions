namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
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
        ///     Sets the contents of the <see cref="File" /> to <paramref name="bytes" />.
        /// </summary>
        IFileManipulator HasBytesContent(byte[] bytes);

        /// <summary>
        ///     Sets the contents of the <see cref="File" /> to <paramref name="contents" />.
        /// </summary>
        IFileManipulator HasStringContent(string contents);
    }
}