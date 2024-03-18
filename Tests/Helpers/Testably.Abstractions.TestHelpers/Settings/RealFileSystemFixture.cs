using Newtonsoft.Json;
using System;
using System.IO;

namespace Testably.Abstractions.TestHelpers.Settings;

public class RealFileSystemFixture
{
	public TestSettingStatus LongRunningTests { get; }
	public TestSettingStatus RealFileSystemTests { get; }

	public RealFileSystemFixture()
	{
		try
		{
			string path = Path.GetFullPath(
				Path.Combine("..", "..", "..", "..", "test.settings.json"));
			string content = File.ReadAllText(path);
			TestEnvironment environment = JsonConvert.DeserializeObject<TestEnvironment>(content)!;

			RealFileSystemTests = environment.RealFileSystemTests;
			LongRunningTests = environment.LongRunningTests;
		}
		catch (Exception)
		{
			RealFileSystemTests = TestSettingStatus.DisabledInDebugMode;
			LongRunningTests = TestSettingStatus.DisabledInDebugMode;
		}
	}
}
