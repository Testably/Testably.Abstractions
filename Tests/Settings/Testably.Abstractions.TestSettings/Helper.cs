using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Testably.Abstractions.TestHelpers.Settings;

namespace Testably.Abstractions.TestSettings;

internal static class Helper
{
	private static readonly JsonSerializerOptions ReadJsonSerializerOptions;
	private static readonly JsonSerializerOptions WriteJsonSerializerOptions;

	static Helper()
	{
		ReadJsonSerializerOptions = new JsonSerializerOptions();
		ReadJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
		WriteJsonSerializerOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			Converters =
			{
				new JsonStringEnumConverter()
			}
		};
	}

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
			return JsonSerializer.Deserialize<TestEnvironment>(content, ReadJsonSerializerOptions)
			       ?? throw new NotSupportedException("The file has an invalid syntax!");
		}
		catch (Exception)
		{
			return new TestEnvironment();
		}
	}

	private static void WriteTestSettings(TestEnvironment environment)
	{
		string content = JsonSerializer.Serialize(environment, WriteJsonSerializerOptions);
		string path = GetTestSettingsPath();
		File.WriteAllText(path, content);
	}
}
