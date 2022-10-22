using System.Runtime.InteropServices;

namespace Testably.Abstractions.AccessControl.Tests.TestHelpers;

public static class Test
{
	public static bool RunsOnWindows
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}