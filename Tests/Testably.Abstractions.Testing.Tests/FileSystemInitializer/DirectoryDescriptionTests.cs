using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class DirectoryDescriptionTests
{
	[Theory]
	[AutoData]
	public async Task Children_ShouldBeSortedAlphabetically(string[] childNames)
	{
		DirectoryDescription sut = new("foo",
			childNames
				.Select(n => new DirectoryDescription(n))
				.Cast<FileSystemInfoDescription>()
				.ToArray());

		await That(sut.Children.Select(c => c.Name)).IsInAscendingOrder()
			.Using(StringComparer.Ordinal);
	}

	[Fact]
	public async Task Index_AccessToMissingChildShouldThrowTestingException()
	{
		DirectoryDescription sut = new("foo");

		void Act()
		{
			_ = sut["bar"];
		}

		await That(Act).ThrowsExactly<TestingException>().WithMessage("*'bar'*").AsWildcard();
	}

	[Fact]
	public async Task Index_ShouldGiveAccessToExistingChildDirectory()
	{
		DirectoryDescription child = new("bar");
		DirectoryDescription sut = new("foo", child);

		FileSystemInfoDescription result = sut["bar"];

		await That(result).IsSameAs(child);
	}

	[Fact]
	public async Task Index_ShouldGiveAccessToNestedChildren()
	{
		FileDescription child3 = new("child3", "some-content");
		DirectoryDescription child2 = new("child2", child3);
		DirectoryDescription child1 = new("child1", child2);
		DirectoryDescription sut = new("foo", child1);

		FileSystemInfoDescription result = sut["child1"]["child2"]["child3"];

		await That(result).IsSameAs(child3);
	}
}
