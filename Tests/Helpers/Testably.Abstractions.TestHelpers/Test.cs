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

	public static void SkipBrittleTestsOnRealTimeSystem(
		ITimeSystem timeSystem, bool condition = true)
	{
		Skip.If(timeSystem is RealTimeSystem && condition,
			"Brittle tests are skipped on the real time system.");
	}
}
