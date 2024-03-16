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
}
