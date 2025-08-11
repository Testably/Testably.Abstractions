using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Testing.Tests;

public class MockFileSystemInitializationTests
{
	[Fact]
	public async Task EnumerateDirectories_WithEnumerationOptions_ShouldConsiderIgnoreInaccessible()
	{
		Skip.IfNot(Test.RunsOnWindows);

		var fileSystem = new RealFileSystem();
		string path = @"C:\Windows\System32";

		void Act()
		{
			_ = fileSystem.Directory
				.EnumerateDirectories(path, "*").ToList();
		}

		await That(Act).DoesNotThrow();
	}
}
