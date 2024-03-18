namespace Testably.Abstractions.TestHelpers.Settings;

public class TestEnvironment
{
	public TestSettingStatus LongRunningTests { get; set; }
		= TestSettingStatus.DisabledInDebugMode;

	public TestSettingStatus RealFileSystemTests { get; set; }
		= TestSettingStatus.DisabledInDebugMode;
}
