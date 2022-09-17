using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Internal;

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