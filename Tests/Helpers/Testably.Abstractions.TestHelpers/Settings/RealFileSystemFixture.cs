using Newtonsoft.Json;
using System;
using System.IO;

namespace Testably.Abstractions.TestHelpers.Settings;

public class RealFileSystemFixture
{
	public bool EnableRealFileSystemTestsInDebugMode { get; }
	public bool IncludeLongRunningTestsAlsoInDebugMode { get; }

	public RealFileSystemFixture()
	{
		string path = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "test.settings.json"));
		string content = File.ReadAllText(path);
		TestSettings settings = JsonConvert.DeserializeObject<TestSettings>(content)
		                        ?? throw new NotSupportedException(
			                        "The file has an invalid syntax!");
		EnableRealFileSystemTestsInDebugMode = settings.EnableRealFileSystemTestsInDebugMode;
		IncludeLongRunningTestsAlsoInDebugMode = settings.IncludeLongRunningTestsAlsoInDebugMode;
	}
}
