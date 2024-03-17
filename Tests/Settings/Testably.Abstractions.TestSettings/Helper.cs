using System;
using System.IO;
using System.Text.Json;

namespace Testably.Abstractions.TestSettings;

internal static class Helper
{
	public static TestHelpers.Settings.TestSettings ChangeTestSettings(
		Action<TestHelpers.Settings.TestSettings> change)
	{
		TestHelpers.Settings.TestSettings settings = ReadTestSettings();
		change(settings);
		WriteTestSettings(settings);
		return settings;
	}

	private static string GetTestSettingsPath() =>
		Path.GetFullPath(Path.Combine("..", "..", "..", "..", "test.settings.json"));

	private static TestHelpers.Settings.TestSettings ReadTestSettings()
	{
		try
		{
			string path = GetTestSettingsPath();
			string content = File.ReadAllText(path);
			return JsonSerializer.Deserialize<TestHelpers.Settings.TestSettings>(content)
			       ?? throw new NotSupportedException("The file has an invalid syntax!");
		}
		catch (Exception)
		{
			return new TestHelpers.Settings.TestSettings();
		}
	}

	private static void WriteTestSettings(TestHelpers.Settings.TestSettings settings)
	{
		string content = JsonSerializer.Serialize(settings, new JsonSerializerOptions
		{
			WriteIndented = true
		});
		string path = GetTestSettingsPath();
		File.WriteAllText(path, content);
	}
}
