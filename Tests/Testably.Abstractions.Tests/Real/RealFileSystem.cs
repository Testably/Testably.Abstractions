using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM
	// ReSharper disable once UnusedMember.Global
	[Collection(nameof(Abstractions.Tests.FileSystem.DriveInfoFactory.RealFileSystemTests))]
	[SystemTest(nameof(Abstractions.Tests.FileSystem.DriveInfoFactory.RealFileSystemTests))]
	public sealed class Tests : FileSystemTests<Abstractions.FileSystem>
	{
		public Tests() : base(new Abstractions.FileSystem())
		{
		}
	}
#endif
}