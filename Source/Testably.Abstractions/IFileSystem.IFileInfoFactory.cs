namespace Testably.Abstractions;

public partial interface IFileSystem
{
    /// <summary>
    ///     Factory for abstracting creation of <see cref="System.IO.FileInfo" />.
    /// </summary>
    public interface IFileInfoFactory : IFileSystemExtensionPoint
    {
        /// <inheritdoc cref="System.IO.FileInfo(string)" />
        IFileInfo New(string path);
    }
}