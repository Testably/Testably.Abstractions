using Newtonsoft.Json;
using System;
using System.IO;

namespace Testably.Abstractions.TestHelpers.Settings;

public class TestSettingsFixture
{
	public TestSettingStatus BrittleTests { get; }
	public TestSettingStatus LongRunningTests { get; }
	public TestSettingStatus RealFileSystemTests { get; }

	public TestSettingsFixture()
	{
		TestEnvironment environment = LoadTestEnvironment();

		RealFileSystemTests = environment.RealFileSystemTests;
		LongRunningTests = environment.LongRunningTests;
		BrittleTests = environment.BrittleTests;
	}

	private static TestEnvironment LoadTestEnvironment()
	{
		try
		{
			string path = Path.GetFullPath(
				Path.Combine("..", "..", "..", "..", "test.settings.json"));
			if (File.Exists(path))
			{
				string content = File.ReadAllText(path);
				TestEnvironment? testEnvironment =
					JsonConvert.DeserializeObject<TestEnvironment>(content);
				if (testEnvironment != null)
				{
					return testEnvironment;
				}
			}
		}
		catch (Exception)
		{
			// Ignore all exceptions while reading the test.settings.json file and use the default settings as fallback.
		}

		return new TestEnvironment();
	}
}
