using NUnit.Framework;
using System;
using System.IO;
using System.Text.Json;

namespace Testably.Abstractions.TestSettings;

public sealed class Settings
{
	/// <summary>
	///     If disabled, the classes for executing tests against the real file system no longer run in DEBUG mode.
	/// </summary>
	[TestCase]
	[Explicit]
	public void DisableRealFileSystemTestsInDebugMode()
	{
		string result = ChangeTestSettings(s =>
			s.EnableRealFileSystemTestsInDebugMode = false);

		Assert.That(result, Is.Not.Null);
	}

	/// <summary>
	///     If enabled, the classes for executing tests against the real file system also run in DEBUG mode.
	/// </summary>
	[TestCase]
	[Explicit]
	public void EnableRealFileSystemTestsInDebugMode()
	{
		string result = ChangeTestSettings(s =>
			s.EnableRealFileSystemTestsInDebugMode = true);

		Assert.That(result, Is.Not.Null);
	}

	/// <summary>
	///     In order to increase the test execution speed during development, long-running tests are disabled per default in
	///     DEBUG mode.
	///     With this setting, the corresponding tests are again skipped.
	/// </summary>
	[TestCase]
	[Explicit]
	public void ExcludeLongRunningTestsAlsoInDebugMode()
	{
		string result = ChangeTestSettings(s =>
			s.IncludeLongRunningTestsAlsoInDebugMode = false);

		Assert.That(result, Is.Not.Null);
	}

	/// <summary>
	///     In order to increase the test execution speed during development, long-running tests are disabled per default in
	///     DEBUG mode.
	///     With this setting, the corresponding tests are no longer skipped.
	/// </summary>
	[TestCase]
	[Explicit]
	public void IncludeLongRunningTestsAlsoInDebugMode()
	{
		string result = ChangeTestSettings(s =>
			s.IncludeLongRunningTestsAlsoInDebugMode = true);

		Assert.That(result, Is.Not.Null);
	}

	private static string ChangeTestSettings(Action<TestHelpers.Settings.TestSettings> change)
	{
		TestHelpers.Settings.TestSettings settings = ReadTestSettings();
		change(settings);
		return WriteTestSettings(settings);
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

	private static string WriteTestSettings(TestHelpers.Settings.TestSettings settings)
	{
		string content = JsonSerializer.Serialize(settings, new JsonSerializerOptions
		{
			WriteIndented = true
		});
		string path = GetTestSettingsPath();
		File.WriteAllText(path, content);
		return content;
	}
}
