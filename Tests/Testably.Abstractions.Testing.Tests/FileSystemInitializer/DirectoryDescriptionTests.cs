using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class DirectoryDescriptionTests
{
	[Fact]
	public void Index_AccessToMissingChildShouldThrowTestingException()
	{
		DirectoryDescription sut = new("foo");

		Exception? exception = Record.Exception(() =>
		{
			_ = sut["bar"];
		});

		exception.Should().BeOfType<TestingException>();
	}

	[Fact]
	public void Index_ShouldGiveAccessToExistingChildDirectory()
	{
		DirectoryDescription child = new("bar");
		DirectoryDescription sut = new("foo", child);

		FileSystemInfoDescription result = sut["bar"];

		result.Should().Be(child);
	}

	[Fact]
	public void Index_ShouldGiveAccessToNestedChildren()
	{
		FileDescription child3 = new("child3", "some-content");
		DirectoryDescription child2 = new("child2", child3);
		DirectoryDescription child1 = new("child1", child2);
		DirectoryDescription sut = new("foo", child1);

		FileSystemInfoDescription result = sut["child1"]["child2"]["child3"];

		result.Should().Be(child3);
	}
}
