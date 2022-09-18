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

    /// <summary>
    ///     The used time system.
    /// </summary>
    public ITimeSystem TimeSystem { get; private set; }

    internal IInMemoryFileSystem InMemoryFileSystem { get; }

    private readonly FileSystemMockCallbackHandler _callbackHandler;
    private readonly DirectoryMock _directoryMock;
    private readonly FileMock _fileMock;
    private readonly PathMock _pathMock;

    /// <summary>
    ///     Initializes the <see cref="FileSystemMock" />.
    /// </summary>
    public FileSystemMock()
    {
        TimeSystem = new TimeSystem();
        Generate = new FileGenerator(this);
        InMemoryFileSystem = new InMemoryFileSystem(this);
        _callbackHandler = new FileSystemMockCallbackHandler();
        _directoryMock = new DirectoryMock(this, _callbackHandler);
        _fileMock = new FileMock(this, _callbackHandler);
        _pathMock = new PathMock(this);
    }

    #region IFileSystem Members

    /// <inheritdoc cref="IFileSystem.Directory" />
    public IFileSystem.IDirectory Directory => _directoryMock;

    /// <inheritdoc cref="IFileSystem.File" />
    public IFileSystem.IFile File => _fileMock;

    /// <inheritdoc cref="IFileSystem.Path" />
    public IFileSystem.IPath Path => _pathMock;

    #endregion
}