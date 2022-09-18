using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing.Internal;

internal static class InMemoryFileSystemHelper
{
    public static DirectoryInfoMock CreateDirectoryInfo(
        FileSystemMock fileSystemMock, string path)
    {
        return new DirectoryInfoMock(path, fileSystemMock);
    }
}