using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

public static class Test
{
	private static Testably.Abstractions.TestHelpers.Test _test = new();

	public static bool IsNetFramework
		=> _test.IsNetFramework;

	public static bool RunsOnLinux
		=> _test.RunsOnLinux;

	public static bool RunsOnMac
		=> _test.RunsOnMac;

	public static bool RunsOnWindows
		=> _test.RunsOnWindows;
}
