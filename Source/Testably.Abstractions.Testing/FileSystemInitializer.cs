namespace Testably.Abstractions.Testing;

/// <summary>
///     Initializes the <see cref="IFileSystem" /> with test data.
/// </summary>
public static partial class FileSystemInitializer
{
    /// <summary>
    ///     Initializes the <see cref="IFileSystem" /> in the working directory with test data.
    /// </summary>
    public static IFileSystemInitializer<TFileSystem> Initialize<TFileSystem>(
        this TFileSystem fileSystem)
        where TFileSystem : IFileSystem
        => fileSystem.InitializeIn(".");

    /// <summary>
    ///     Initializes the <see cref="IFileSystem" /> in the <paramref name="basePath" /> with test data.
    /// </summary>
    public static IFileSystemInitializer<TFileSystem> InitializeIn<TFileSystem>(
        this TFileSystem fileSystem,
        string basePath)
        where TFileSystem : IFileSystem
    {
        fileSystem.Directory.CreateDirectory(basePath);
        return new Initializer<TFileSystem>(fileSystem, basePath);
    }
}