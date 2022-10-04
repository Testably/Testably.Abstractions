using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM
    // ReSharper disable once UnusedMember.Global
    [Collection(nameof(RealFileSystem))]
    [SystemTest(nameof(RealFileSystem))]
    public sealed class Tests : FileSystemTests<FileSystem>
    {
        public Tests() : base(new FileSystem())
        {
        }
    }
#endif
}