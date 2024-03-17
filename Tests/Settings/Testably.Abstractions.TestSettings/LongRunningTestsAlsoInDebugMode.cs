using NUnit.Framework;

namespace Testably.Abstractions.TestSettings;

public sealed class LongRunningTestsAlsoInDebugMode
{
	/// <summary>
	///     In order to increase the test execution speed during development, long-running tests are disabled per default in
	///     DEBUG mode.
	///     With this setting, the corresponding tests are again skipped.
	/// </summary>
	[TestCase]
	[Explicit]
	public void Exclude()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.IncludeLongRunningTestsAlsoInDebugMode = false);

		Assert.Pass("Long-running tests are excluded in DEBUG mode.");
	}

	/// <summary>
	///     In order to increase the test execution speed during development, long-running tests are disabled per default in
	///     DEBUG mode.
	///     With this setting, the corresponding tests are no longer skipped.
	/// </summary>
	[TestCase]
	[Explicit]
	public void Include()
	{
		TestHelpers.Settings.TestSettings result = Helper.ChangeTestSettings(s =>
			s.IncludeLongRunningTestsAlsoInDebugMode = true);

		if (!result.EnableRealFileSystemTestsInDebugMode)
		{
			Assert.Warn("""
			            Long-running tests are included in DEBUG mode.
			            But the tests against the real file system are still disabled.
			            """);
		}
		else
		{
			Assert.Pass("Long-running tests are included in DEBUG mode.");
		}
	}

	[TestCase]
	public void TestDummy_SoThatExplicitTestsAreIgnored()
	{
		Assert.Pass();
	}
}
