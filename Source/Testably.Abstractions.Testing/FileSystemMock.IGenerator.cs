namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The file generator for the <see cref="FileSystemMock" />
    /// </summary>
    public interface IGenerator : IFileSystem.IFileSystemExtensionPoint
    {
    }
}