using System.Runtime.InteropServices;

namespace Testably.Abstractions.AccessControl.Tests.TestHelpers;

public static class Test
{
	public static bool RunsOnWindows
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	public static void SkipIfLongRunningTestsShouldBeSkipped(IFileSystem fileSystem)
	{
#if DEBUG && !INCLUDE_LONGRUNNING_TESTS_ALSO_IN_DEBUG_MODE
		Skip.If(fileSystem is FileSystem,
			"Long-Running tests are skipped in DEBUG mode unless the build constant 'INCLUDE_LONG_RUNNING_TESTS_ALSO_IN_DEBUG_MODE' is set.");
#endif
	}
}