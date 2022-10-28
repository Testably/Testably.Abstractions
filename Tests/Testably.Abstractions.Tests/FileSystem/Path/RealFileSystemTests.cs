#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once UnusedMember.Global
[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests : FileSystemPathTests<RealFileSystem>
{
	public RealFileSystemTests() : base(new RealFileSystem())
	{
	}
}
#endif