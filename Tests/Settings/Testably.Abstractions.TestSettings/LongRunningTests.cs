using NUnit.Framework;
using Testably.Abstractions.TestHelpers.Settings;

namespace Testably.Abstractions.TestSettings;

public sealed class LongRunningTests
{
	/// <summary>
	///     In order to increase the test execution speed during development, long-running tests are disabled per default in
	///     DEBUG mode.
	///     With this setting, the corresponding tests are again skipped.
	/// </summary>
	[TestCase]
	[Explicit]
	public void DisableAlways()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.LongRunningTests = TestSettingStatus.AlwaysDisabled);

		Assert.Pass("Long-running tests are always disabled.");
	}

	/// <summary>
	///     In order to increase the test execution speed during development, long-running tests are disabled per default in
	///     DEBUG mode.
	///     With this setting, the corresponding tests are again skipped.
	/// </summary>
	[TestCase]
	[Explicit]
	public void DisableInDebugMode()
	{
		TestEnvironment result = Helper.ChangeTestSettings(s =>
			s.LongRunningTests = TestSettingStatus.DisabledInDebugMode);

		if (result.RealFileSystemTests == TestSettingStatus.AlwaysDisabled)
		{
			Assert.Warn("""
			            Long-running tests are always enabled.
			            But the tests against the real file system are always disabled.
			            """);
		}
		else
		{
			Assert.Pass("Long-running tests are disabled in DEBUG mode.");
		}
	}

	/// <summary>
	///     In order to increase the test execution speed during development, long-running tests are disabled per default in
	///     DEBUG mode.
	///     With this setting, the corresponding tests are no longer skipped.
	/// </summary>
	[TestCase]
	[Explicit]
	public void EnableAlways()
	{
		TestEnvironment result = Helper.ChangeTestSettings(s =>
			s.LongRunningTests = TestSettingStatus.AlwaysEnabled);

		if (result.RealFileSystemTests != TestSettingStatus.AlwaysEnabled)
		{
			Assert.Warn("""
			            Long-running tests are always enabled.
			            But the tests against the real file system are not always enabled.
			            """);
		}
		else
		{
			Assert.Pass("Long-running tests are always enabled.");
		}
	}

	[TestCase]
	public void TestDummy_SoThatExplicitTestsAreIgnored()
	{
		Assert.Pass();
	}
}
