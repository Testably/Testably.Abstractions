namespace Testably.Abstractions.TestHelpers.Settings;

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
