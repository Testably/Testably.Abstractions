using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.TestHelpers;

public static class Test
{
	public static bool RunsOnLinux
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static bool RunsOnWindows
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	public static void SkipIfLongRunningTestsShouldBeSkipped(IFileSystem fileSystem)
	{
#if DEBUG && !INCLUDE_LONGRUNNING_TESTS_ALSO_IN_DEBUG_MODE
		Skip.If(fileSystem is FileSystem,
			"Long-Running tests are skipped in DEBUG mode unless the build constant 'INCLUDE_LONG_RUNNING_TESTS_ALSO_IN_DEBUG_MODE' is set.");
#endif
	}

	public static void SkipIfTestsOnRealFileSystemShouldBeSkipped(IFileSystem fileSystem)
	{
#if NCRUNCH
		Skip.If(fileSystem is FileSystem, "NCrunch should not test the real file system.");
#endif
#if DEBUG && SKIP_TESTS_ON_REAL_FILESYSTEM
		Skip.If(fileSystem is FileSystem,
			"Tests against real FileSystem are skipped in DEBUG mode with the build constant 'SKIP_TESTS_ON_REAL_FILESYSTEM'.");
#endif
	}
}