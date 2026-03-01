namespace Testably.Abstractions.TestHelpers;

public static class Settings
{
	/// <summary>
	///     Affects some tests, that are brittle on the real file system.
	/// </summary>
	/// <remarks>Per default, they are <see cref="TestSettingStatus.AlwaysDisabled" />.</remarks>
	public static TestSettingStatus BrittleTests { get; set; }
		= TestSettingStatus.AlwaysDisabled;

	/// <summary>
	///     Affects some tests, that take a long time to run against the real file system (e.g. timeout).
	/// </summary>
	/// <remarks>Per default, they are <see cref="TestSettingStatus.DisabledInDebugMode" />.</remarks>
	public static TestSettingStatus LongRunningTests { get; set; }
		= TestSettingStatus.DisabledInDebugMode;

	/// <summary>
	///     Affects all tests against the real file system.
	/// </summary>
	/// <remarks>Per default, they are <see cref="TestSettingStatus.DisabledInDebugMode" />.</remarks>
	public static TestSettingStatus RealFileSystemTests { get; set; }
		= TestSettingStatus.DisabledInDebugMode;
	
	public enum TestSettingStatus
	{
		/// <summary>
		///     The tests are always enabled.
		/// </summary>
		AlwaysEnabled,

		/// <summary>
		///     The tests are only disabled in DEBUG mode.
		/// </summary>
		DisabledInDebugMode,

		/// <summary>
		///     The tests are always disabled.
		/// </summary>
		AlwaysDisabled,
	}
}
