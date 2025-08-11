using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests;

public class MockFileSystemInitializationTests
{
	private readonly ITestOutputHelper _testOutputHelper;
	public MockFileSystemInitializationTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task EnumerateDirectories_WithEnumerationOptions_ShouldConsiderIgnoreInaccessible()
	{
		Skip.IfNot(Test.RunsOnWindows);

		var sb1 = new StringBuilder();
		var sb2 = new StringBuilder();

		RealFileSystem fileSystem = new();
		string path = @"C:\Windows\System32";
		EnumerationOptions enumerationOptions = new()
		{
			IgnoreInaccessible = true,
			RecurseSubdirectories = false,
		};

		foreach (var directory in fileSystem.Directory.GetDirectories(path, "*", enumerationOptions))
		{
			try
			{
				fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(directory, "_test_"));
				sb2.AppendLine("Access allowed for: " + directory);
			}
			catch (UnauthorizedAccessException)
			{
				sb1.AppendLine("Access denied for: " + directory);
			}
			catch (Exception e)
			{
				sb2.AppendLine("Error for: " + directory + " (" + e.Message + ")");
			}
		}

		var result = sb1.ToString();
		_testOutputHelper.WriteLine(result);
		_testOutputHelper.WriteLine(sb2.ToString());
		await That(result).IsEmpty();
		await That(false).IsTrue();
	}
#endif
}
