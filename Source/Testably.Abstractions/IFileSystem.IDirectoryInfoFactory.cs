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
    }
}