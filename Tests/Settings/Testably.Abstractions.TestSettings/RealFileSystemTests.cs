using NUnit.Framework;
using Testably.Abstractions.TestHelpers.Settings;

namespace Testably.Abstractions.TestSettings;

public sealed class RealFileSystemTests
{
	/// <summary>
	///     If enabled, the classes for executing tests against the real file system also run in DEBUG mode.
	/// </summary>
	[TestCase]
	[Explicit]
	public void DisableAlways()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.RealFileSystemTests = TestSettingStatus.AlwaysDisabled);

		Assert.Pass("Tests against the real file system are enabled in DEBUG mode.");
	}

	/// <summary>
	///     If disabled, the classes for executing tests against the real file system no longer run in DEBUG mode.
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
	///     If enabled, the classes for executing tests against the real file system also run in DEBUG mode.
	/// </summary>
	[TestCase]
	[Explicit]
	public void EnableAlways()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.RealFileSystemTests = TestSettingStatus.AlwaysEnabled);

		Assert.Pass("Tests against the real file system are enabled in DEBUG mode.");
	}

	[TestCase]
	public void TestDummy_SoThatExplicitTestsAreIgnored()
	{
		Assert.Pass();
	}
}
