using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing.Internal;

internal static class InMemoryFileSystemHelper
{
    public static MockDirectoryInfo CreateDirectoryInfo(
        FileSystemMock fileSystemMock, string path)
    {
        return new MockDirectoryInfo(fileSystemMock, path);
    }
}