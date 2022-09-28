using Testably.Abstractions.Tests.TestHelpers.Attributes;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [Collection(nameof(RealFileSystemTestAttribute))]
    [RealFileSystemTest]
    public sealed class PathTests : FileSystemPathTests<FileSystem>
    {
        public PathTests() : base(new FileSystem())
        {
        }
    }
}