using System.IO;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
    /// <summary>
    ///     Factory for abstracting creation of <see cref="System.IO.DirectoryInfo" />.
    /// </summary>
    public interface IDirectoryInfoFactory : IFileSystemExtensionPoint
    {
        /// <inheritdoc cref="System.IO.DirectoryInfo(string)" />
        IDirectoryInfo New(string path);

        /// <summary>
        ///     Wraps the <paramref name="directoryInfo"/> to the testably interface <see cref="IDirectoryInfo"/>.
        /// </summary>
        IDirectoryInfo Wrap(DirectoryInfo directoryInfo);
    }
}