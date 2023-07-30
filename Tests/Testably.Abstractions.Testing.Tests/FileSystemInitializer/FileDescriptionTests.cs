using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class FileDescriptionTests
{
	[Fact]
	public void Index_AccessShouldThrowTestingException()
	{
		FileDescription sut = new("foo");

		Exception? exception = Record.Exception(() =>
		{
			_ = sut["bar"];
		});

		exception.Should().BeOfType<TestingException>();
	}
}
