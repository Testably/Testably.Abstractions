using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the file system. Implements <see cref="IFileSystem" />.
/// </summary>
public sealed partial class FileSystemMock : IFileSystem
{
    /// <summary>
    ///     The callback handler for the <see cref="FileSystemMock" />.
    /// </summary>
    public ICallbackHandler On => _callbackHandler;

    /// <summary>
    ///     The generator to create test helper files.
    /// </summary>
    public IGenerator Generate { get; }

    private readonly FileSystemMockCallbackHandler _callbackHandler;
    private readonly PathMock _pathMock;
    private readonly FileMock _fileMock;

    /// <summary>
    ///     Initializes the <see cref="FileSystemMock" />.
    /// </summary>
    public FileSystemMock()
    {
        _callbackHandler = new FileSystemMockCallbackHandler();
        _pathMock = new PathMock(this);
        _fileMock = new FileMock(this, _callbackHandler);
        Generate = new FileGenerator(this);
    }

    #region IFileSystem Members

    /// <inheritdoc cref="IFileSystem.Path" />
    public IFileSystem.IPath Path => _pathMock;

    /// <inheritdoc cref="IFileSystem.File" />
    public IFileSystem.IFile File => _fileMock;

    #endregion
}