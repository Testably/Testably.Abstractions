using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Xunit;

namespace Testably.Abstractions.TestHelpers;

public class Test
{
	public static bool IsNet7OrGreater
#if NET7_0_OR_GREATER
		=> true;
#else
		=> false;
#endif
	public bool IsNetFramework { get; }

	public bool RunsOnLinux { get; }

	public bool RunsOnMac { get; }

	public bool RunsOnWindows { get; }

	public Test()
	{
		RunsOnLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		RunsOnMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		RunsOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		IsNetFramework = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");
	}

	public static void SkipBrittleTestsOnRealFileSystem(
		IFileSystem fileSystem, bool condition = true)
	{
		Skip.If(fileSystem is RealFileSystem && condition,
			"Brittle tests are skipped on the real file system.");
	}

	public static void SkipBrittleTestsOnRealTimeSystem(
		ITimeSystem timeSystem, bool condition = true)
	{
		Skip.If(timeSystem is RealTimeSystem && condition,
			"Brittle tests are skipped on the real time system.");
	}

	public static void SkipIfLongRunningTestsShouldBeSkipped(IFileSystem fileSystem)
	{
#if DEBUG && !INCLUDE_LONGRUNNING_TESTS_ALSO_IN_DEBUG_MODE
		Skip.If(fileSystem is RealFileSystem,
			"Long-Running tests are skipped in DEBUG mode unless the build constant 'INCLUDE_LONG_RUNNING_TESTS_ALSO_IN_DEBUG_MODE' is set.");
#endif
		// ReSharper disable once CommentTypo
		// Do nothing when in release mode or `INCLUDE_LONGRUNNING_TESTS_ALSO_IN_DEBUG_MODE` is set
	}

	public static void SkipIfTestsOnRealFileSystemShouldBeSkipped(IFileSystem fileSystem)
	{
#if NCRUNCH
		Skip.If(fileSystem is RealFileSystem, "NCrunch should not test the real file system.");
#endif
#if DEBUG && SKIP_TESTS_ON_REAL_FILESYSTEM
		Skip.If(fileSystem is RealFileSystem,
			"Tests against real FileSystem are skipped in DEBUG mode with the build constant 'SKIP_TESTS_ON_REAL_FILESYSTEM'.");
#endif
		// Do nothing when in release mode or `SKIP_TESTS_ON_REAL_FILESYSTEM` is not set
	}
}
