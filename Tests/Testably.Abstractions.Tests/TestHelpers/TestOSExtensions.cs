namespace Testably.Abstractions.Tests.TestHelpers;

public static class TestOSExtensions
{
	public static bool RunsOn(this Test test, TestOS operatingSystem)
		=> operatingSystem.HasFlag(TestOS.Linux) && test.RunsOnLinux ||
		   operatingSystem.HasFlag(TestOS.Mac) && test.RunsOnMac ||
		   operatingSystem.HasFlag(TestOS.Windows) && test.RunsOnWindows;
}
