using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

public static class Test
{
	public static bool RunsOnLinux
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static bool RunsOnMac
		=> RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	public static bool RunsOnWindows
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
