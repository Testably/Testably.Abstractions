using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.Models;

internal class DirectoryInfoWrapper : IFileSystem.IDirectoryInfo
{
    private readonly DirectoryInfo _instance;
    private readonly IFileSystem _fileSystem;

    internal DirectoryInfoWrapper(DirectoryInfo instance, IFileSystem fileSystem)
    {
        _instance = instance;
        _fileSystem = fileSystem;
    }

    [return: NotNullIfNotNull("instance")]
    internal static DirectoryInfoWrapper? FromDirectoryInfo(DirectoryInfo? instance,
        IFileSystem fileSystem)
    {
        if (instance == null)
        {
            return null;
        }

        return new DirectoryInfoWrapper(instance, fileSystem);
    }
}