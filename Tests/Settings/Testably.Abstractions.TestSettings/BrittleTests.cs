using NUnit.Framework;
using Testably.Abstractions.TestHelpers.Settings;

namespace Testably.Abstractions.TestSettings;

public sealed class BrittleTests
{
	/// <summary>
	///     Some tests are brittle on the real file system.
	///     <para />
	///     Always disable these tests!
	/// </summary>
	[TestCase]
	[Explicit]
	public void DisableAlways()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.BrittleTests = TestSettingStatus.AlwaysDisabled);

		Assert.Pass("Brittle tests are always disabled.");
	}

	/// <summary>
	///     Some tests are brittle on the real file system.
	///     <para />
	///     Disable these tests in DEBUG mode!
	/// </summary>
	[TestCase]
	[Explicit]
	public void DisableInDebugMode()
	{
		TestEnvironment result = Helper.ChangeTestSettings(s =>
			s.BrittleTests = TestSettingStatus.DisabledInDebugMode);

		if (result.RealFileSystemTests == TestSettingStatus.AlwaysDisabled)
		{
			Assert.Warn("""
			            Brittle tests are disabled in DEBUG mode.
			            But the tests against the real file system are always disabled.
			            """);
		}
		else
		{
			Assert.Pass("Brittle tests are disabled in DEBUG mode.");
		}
	}

	/// <summary>
	///     Some tests are brittle on the real file system.
	///     <para />
	///     Always enable these tests!
	/// </summary>
	[TestCase]
	[Explicit]
	public void EnableAlways()
	{
		TestEnvironment result = Helper.ChangeTestSettings(s =>
			s.BrittleTests = TestSettingStatus.AlwaysEnabled);

		if (result.RealFileSystemTests != TestSettingStatus.AlwaysEnabled)
		{
			Assert.Warn("""
			            Brittle tests are always enabled.
			            But the tests against the real file system are not always enabled.
			            """);
		}
		else
		{
			Assert.Pass("Brittle tests are always enabled.");
		}
	}

	[TestCase]
	public void TestDummy_SoThatExplicitTestsAreIgnored()
	{
		Assert.Pass();
	}
}
