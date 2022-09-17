using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class PathFileSystem : PathSystem
    {
        public PathFileSystem(FileSystem fileSystem)
            : base(fileSystem)
        {
        }
    }
}