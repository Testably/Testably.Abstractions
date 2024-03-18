namespace Testably.Abstractions.TestHelpers.Settings;

public class TestEnvironment
{
	/// <summary>
	///     Affects some tests, that take a long time to run against the real file system (e.g. timeout).
	/// </summary>
	/// <remarks>Per default, they are <see cref="TestSettingStatus.DisabledInDebugMode" />.</remarks>
	public TestSettingStatus LongRunningTests { get; set; }
		= TestSettingStatus.DisabledInDebugMode;

	/// <summary>
	///     Affects all tests against the real file system.
	/// </summary>
	/// <remarks>Per default, they are <see cref="TestSettingStatus.DisabledInDebugMode" />.</remarks>
	public TestSettingStatus RealFileSystemTests { get; set; }
		= TestSettingStatus.DisabledInDebugMode;
}
