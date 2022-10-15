#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM
using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.FileSystem;

[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
[SystemTest(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests : FileSystemTests<Abstractions.FileSystem>
{
	public RealFileSystemTests() : base(new Abstractions.FileSystem())
	{
	}
}
#endif