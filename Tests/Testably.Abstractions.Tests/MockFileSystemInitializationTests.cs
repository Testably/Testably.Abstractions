using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests;

public class MockFileSystemInitializationTests
{
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task EnumerateDirectories_WithEnumerationOptions_ShouldConsiderIgnoreInaccessible()
	{
		Skip.IfNot(Test.RunsOnWindows);

		var sb = new StringBuilder();

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
				sb.AppendLine("Access allowed for: " + directory);
			}
			catch (UnauthorizedAccessException)
			{
				sb.AppendLine("Access denied for: " + directory);
			}
			catch (Exception e)
			{
				sb.AppendLine("Error for: " + directory + " (" + e.Message + ")");
			}
		}

		var result = sb.ToString();
		await That(result).IsEmpty();
	}
#endif
}
