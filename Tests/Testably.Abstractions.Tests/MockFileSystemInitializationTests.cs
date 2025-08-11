using System.IO;
using System.Linq;

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
	public void EnumerateDirectories_WithEnumerationOptions_ShouldConsiderIgnoreInaccessible()
	{
		Skip.IfNot(Test.RunsOnWindows);

		RealFileSystem fileSystem = new();
		string path = @"C:\Windows";
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
				_testOutputHelper.WriteLine("Access allowed for: " + directory);
			}
			catch (UnauthorizedAccessException)
			{
				_testOutputHelper.WriteLine("Access denied for: " + directory);
			}
			catch (Exception e)
			{
				_testOutputHelper.WriteLine("Error for: " + directory + " (" + e.Message + ")");
			}
		}
	}
#endif
}
