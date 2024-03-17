using Newtonsoft.Json;
using System;
using System.IO;

namespace Testably.Abstractions.TestHelpers.Settings;

public class RealFileSystemFixture
{
	public RealFileSystemFixture()
	{
		var path = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "test.settings.json"));
		string content = File.ReadAllText(path);
		var settings = JsonConvert.DeserializeObject<TestSettings>(content)
		               ?? throw new NotSupportedException("The file has an invalid syntax!");
		EnableRealFileSystemTestsInDebugMode = settings.EnableRealFileSystemTestsInDebugMode;
		IncludeLongRunningTestsAlsoInDebugMode = settings.IncludeLongRunningTestsAlsoInDebugMode;
	}
	public bool EnableRealFileSystemTestsInDebugMode { get; }
	public bool IncludeLongRunningTestsAlsoInDebugMode { get; }
}
