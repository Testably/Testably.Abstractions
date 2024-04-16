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

	public static bool RunsOn(this Test test, TestOs operatingSystem)
		=> (operatingSystem.HasFlag(TestOs.Linux) && test.RunsOnLinux) ||
		   (operatingSystem.HasFlag(TestOs.Mac) && test.RunsOnMac) ||
		   (operatingSystem.HasFlag(TestOs.Windows) && test.RunsOnWindows);
}
