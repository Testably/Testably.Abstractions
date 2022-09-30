namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A <see cref="IFileSystem.IDriveInfo" /> which allows to be manipulated.
    /// </summary>
    public interface IDriveInfoMock : IFileSystem.IDriveInfo
    {
        /// <summary>
        ///     Changes the total size of the <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IDriveInfoMock SetTotalSize(long totalSize);
    }
}