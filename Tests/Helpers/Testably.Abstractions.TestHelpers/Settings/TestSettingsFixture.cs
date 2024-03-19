using Newtonsoft.Json;
using System;
using System.IO;

namespace Testably.Abstractions.TestHelpers.Settings;

public class TestSettingsFixture
{
	public TestSettingStatus LongRunningTests { get; }
	public TestSettingStatus BrittleTests { get; }
	public TestSettingStatus RealFileSystemTests { get; }

	public TestSettingsFixture()
	{
		try
		{
			string path = Path.GetFullPath(
				Path.Combine("..", "..", "..", "..", "test.settings.json"));
			string content = File.ReadAllText(path);
			TestEnvironment environment = JsonConvert.DeserializeObject<TestEnvironment>(content)!;

			RealFileSystemTests = environment.RealFileSystemTests;
			LongRunningTests = environment.LongRunningTests;
			BrittleTests = environment.BrittleTests;
		}
		catch (Exception)
		{
			RealFileSystemTests = TestSettingStatus.DisabledInDebugMode;
			LongRunningTests = TestSettingStatus.DisabledInDebugMode;
			BrittleTests = TestSettingStatus.DisabledInDebugMode;
		}
	}
}
