#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM
using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
	// ReSharper disable once UnusedMember.Global
	[Collection(nameof(RealFileSystem))]
	[SystemTest(nameof(RealFileSystem))]
	public sealed class PathTests : FileSystemPathTests<FileSystem>
	{
		public PathTests() : base(new FileSystem())
		{
		}
	}
}
#endif