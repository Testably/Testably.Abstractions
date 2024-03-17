using NUnit.Framework;

namespace Testably.Abstractions.TestSettings;

public sealed class RealFileSystemTestsInDebugMode
{
	/// <summary>
	///     If disabled, the classes for executing tests against the real file system no longer run in DEBUG mode.
	/// </summary>
	[TestCase]
	[Explicit]
	public void Disable()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.EnableRealFileSystemTestsInDebugMode = false);

		Assert.Pass("Tests against the real file system are disabled in DEBUG mode.");
	}

	/// <summary>
	///     If enabled, the classes for executing tests against the real file system also run in DEBUG mode.
	/// </summary>
	[TestCase]
	[Explicit]
	public void Enable()
	{
		_ = Helper.ChangeTestSettings(s =>
			s.EnableRealFileSystemTestsInDebugMode = true);

		Assert.Pass("Tests against the real file system are enabled in DEBUG mode.");
	}

	[TestCase]
	public void TestDummy_SoThatExplicitTestsAreIgnored()
	{
		Assert.Pass();
	}
}
