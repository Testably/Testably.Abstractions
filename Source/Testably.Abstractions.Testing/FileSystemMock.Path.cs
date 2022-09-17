using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class PathMock : PathSystem
    {
        internal PathMock(FileSystemMock fileSystem)
            : base(fileSystem)
        {
        }
    }
}