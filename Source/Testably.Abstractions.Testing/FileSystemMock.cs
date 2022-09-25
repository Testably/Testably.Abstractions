namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the file system. Implements <see cref="IFileSystem" />.
/// </summary>
public sealed partial class FileSystemMock : IFileSystem
{
    /// <summary>
    ///     The callback handler for the <see cref="FileSystemMock" />.
    /// </summary>
    public ICallbackHandler On
        => _callbackHandler;

    /// <summary>
    ///     The used random system.
    /// </summary>
    public IRandomSystem RandomSystem { get; }

    /// <summary>
    ///     The used time system.
    /// </summary>
    public ITimeSystem TimeSystem { get; }

    internal IInMemoryFileSystem FileSystemContainer { get; }

    private readonly FileSystemMockCallbackHandler _callbackHandler;
    private readonly DirectoryMock _directoryMock;
    private readonly FileMock _fileMock;
    private readonly PathMock _pathMock;

    /// <summary>
    ///     Initializes the <see cref="FileSystemMock" />.
    /// </summary>
    public FileSystemMock()
    {
        RandomSystem = new RandomSystem();
        TimeSystem = new TimeSystemMock(TimeProvider.Now());
        FileSystemContainer = new InMemoryFileSystem(this);
        _callbackHandler = new FileSystemMockCallbackHandler();
        _directoryMock = new DirectoryMock(this, _callbackHandler);
        _fileMock = new FileMock(this, _callbackHandler);
        _pathMock = new PathMock(this);
        DirectoryInfo = new DirectoryInfoFactoryMock(this, _callbackHandler);
        FileInfo = new FileInfoFactoryMock(this, _callbackHandler);
    }

    #region IFileSystem Members

    /// <inheritdoc cref="IFileSystem.Directory" />
    public IFileSystem.IDirectory Directory
        => _directoryMock;

    /// <inheritdoc cref="IFileSystem.DirectoryInfo" />
    public IFileSystem.IDirectoryInfoFactory DirectoryInfo { get; }

    /// <inheritdoc cref="IFileSystem.File" />
    public IFileSystem.IFile File
        => _fileMock;

    /// <inheritdoc cref="IFileSystem.FileInfo" />
    public IFileSystem.IFileInfoFactory FileInfo { get; }

    /// <inheritdoc cref="IFileSystem.Path" />
    public IFileSystem.IPath Path
        => _pathMock;

    #endregion
}