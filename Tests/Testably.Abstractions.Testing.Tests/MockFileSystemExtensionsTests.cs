using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests;

public class MockFileSystemExtensionsTests
{
	[Fact]
	public void GetDefaultDrive_WithoutDrives_ShouldThrowInvalidOperationException()
	{
		MockFileSystem fileSystem = new();
		(fileSystem.Storage as InMemoryStorage)?.RemoveDrive(string.Empty.PrefixRoot(fileSystem));

		Exception? exception = Record.Exception(() =>
		{
			fileSystem.GetDefaultDrive();
		});

		exception.Should().BeOfType<InvalidOperationException>();
	}
}
