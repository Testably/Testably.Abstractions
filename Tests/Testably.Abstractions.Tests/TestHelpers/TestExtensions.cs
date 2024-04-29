namespace Testably.Abstractions.Tests.TestHelpers;

internal static class TestExtensions
{
	public static T DependsOnOS<T>(this Test test, T windows, T macOS, T linux)
	{
		if (test.RunsOnWindows)
		{
			return windows;
		}

		if (test.RunsOnMac)
		{
			return macOS;
		}

		return linux;
	}

	public static bool RunsOn(this Test test, TestOS operatingSystem)
		=> (operatingSystem.HasFlag(TestOS.Linux) && test.RunsOnLinux) ||
		   (operatingSystem.HasFlag(TestOS.Mac) && test.RunsOnMac) ||
		   (operatingSystem.HasFlag(TestOS.Windows) &&
		    test is { RunsOnWindows: true, IsNetFramework: false }) ||
		   (operatingSystem.HasFlag(TestOS.Framework) && test.IsNetFramework);
}
