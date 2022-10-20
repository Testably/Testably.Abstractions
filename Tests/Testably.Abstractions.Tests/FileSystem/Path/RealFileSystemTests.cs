#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

namespace Testably.Abstractions.Tests.FileSystem.Path;

[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests : FileSystemPathTests<Abstractions.FileSystem>
{
	public RealFileSystemTests() : base(new Abstractions.FileSystem())
	{
	}
}
#endif