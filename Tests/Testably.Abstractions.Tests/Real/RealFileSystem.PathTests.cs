using Testably.Abstractions.Tests.TestHelpers.Attributes;
using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [Collection(nameof(RealFileSystemTestAttribute))]
    [RealFileSystemTest]
    [SystemTest(nameof(RealFileSystem))]
    public sealed class PathTests : FileSystemPathTests<FileSystem>
    {
        public PathTests() : base(new FileSystem())
        {
        }
    }
}