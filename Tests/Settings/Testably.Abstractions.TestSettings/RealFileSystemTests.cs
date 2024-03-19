using NUnit.Framework;
using Testably.Abstractions.TestHelpers.Settings;

namespace Testably.Abstractions.TestSettings;

public sealed class RealFileSystemTests
{
	/// <summary>
	///     All tests against the real file system.
	///     <para />
	///     Always disable these tests!
	/// </summary>
	[TestCase]
	[Explicit]
	public void DisableAlways()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.RealFileSystemTests = TestSettingStatus.AlwaysDisabled);

		Assert.Pass("Tests against the real file system are always disabled.");
	}

	/// <summary>
	///     All tests against the real file system.
	///     <para />
	///     Disable these tests in DEBUG mode!
	/// </summary>
	[TestCase]
	[Explicit]
	public void DisableInDebugMode()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.RealFileSystemTests = TestSettingStatus.DisabledInDebugMode);

		Assert.Pass("Tests against the real file system are disabled in DEBUG mode.");
	}

	/// <summary>
	///     All tests against the real file system.
	///     <para />
	///     Always enable these tests!
	/// </summary>
	[TestCase]
	[Explicit]
	public void EnableAlways()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.RealFileSystemTests = TestSettingStatus.AlwaysEnabled);

		Assert.Pass("Tests against the real file system are always enabled.");
	}

	[TestCase]
	public void TestDummy_SoThatExplicitTestsAreIgnored()
	{
		Assert.Pass();
	}
}
