using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Testably.Abstractions.TestHelpers.Settings;

namespace Testably.Abstractions.TestSettings;

internal static class Helper
{
	public static TestEnvironment ChangeTestSettings(
		Action<TestEnvironment> change)
	{
		TestEnvironment environment = ReadTestSettings();
		change(environment);
		WriteTestSettings(environment);
		return environment;
	}

	private static string GetTestSettingsPath() =>
		Path.GetFullPath(Path.Combine("..", "..", "..", "..", "test.settings.json"));

	private static TestEnvironment ReadTestSettings()
	{
		try
		{
			string path = GetTestSettingsPath();
			string content = File.ReadAllText(path);
			JsonSerializerOptions options = new JsonSerializerOptions();
			options.Converters.Add(new JsonStringEnumConverter());
			return JsonSerializer.Deserialize<TestEnvironment>(content, options)
			       ?? throw new NotSupportedException("The file has an invalid syntax!");
		}
		catch (Exception)
		{
			return new TestEnvironment();
		}
	}

	private static void WriteTestSettings(TestEnvironment environment)
	{
		string content = JsonSerializer.Serialize(environment, new JsonSerializerOptions
		{
			WriteIndented = true,
			Converters =
			{
				new JsonStringEnumConverter()
			}
		});
		string path = GetTestSettingsPath();
		File.WriteAllText(path, content);
	}
}
