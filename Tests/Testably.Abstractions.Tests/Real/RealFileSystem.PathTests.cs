namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [Collection(RealFileSystemCollection)]
    public sealed class PathTests : FileSystemPathTests<FileSystem>
    {
        public PathTests() : base(new FileSystem())
        {
        }
    }
}