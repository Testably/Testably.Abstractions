using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests;

public class MockFileSystemInitializationTests
{
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task EnumerateDirectories_WithEnumerationOptions_ShouldConsiderIgnoreInaccessible(
		bool ignoreInaccessible)
	{
		Skip.IfNot(Test.RunsOnWindows);

		RealFileSystem fileSystem = new();
		string path = @"C:\Windows\System32";
		EnumerationOptions enumerationOptions = new()
		{
			IgnoreInaccessible = ignoreInaccessible,
			RecurseSubdirectories = true,
		};

		void Act()
		{
			_ = fileSystem.Directory
				.EnumerateDirectories(path, "*", enumerationOptions).ToList();
		}

		await That(Act).DoesNotThrow();
	}
#endif
}
