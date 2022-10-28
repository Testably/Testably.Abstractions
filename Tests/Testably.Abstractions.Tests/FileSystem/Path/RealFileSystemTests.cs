#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

namespace Testably.Abstractions.Tests.FileSystem.Path;

[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests : FileSystemPathTests<Abstractions.RealFileSystem>
{
	public RealFileSystemTests() : base(new Abstractions.RealFileSystem())
	{
	}
}
#endif