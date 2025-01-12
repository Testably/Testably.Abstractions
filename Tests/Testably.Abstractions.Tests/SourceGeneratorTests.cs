using System.Linq;

namespace Testably.Abstractions.Tests;

public sealed class SourceGeneratorTests
{
	[Fact]
	public void SourceGeneratorTestsAreCreated()
	{
		int mockFileSystemTestsCount = typeof(SourceGeneratorTests).Assembly.GetTypes()
			.Count(x => string.Equals(x.Name, "MockFileSystemTests", StringComparison.Ordinal));

		mockFileSystemTestsCount.Should().BeGreaterThan(100);
	}
}
