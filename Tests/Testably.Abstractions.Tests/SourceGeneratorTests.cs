using System.Linq;

namespace Testably.Abstractions.Tests;

public sealed class SourceGeneratorTests
{
	[Fact]
	public async Task SourceGeneratorTestsAreCreated()
	{
		int mockFileSystemTestsCount = typeof(SourceGeneratorTests).Assembly.GetTypes()
			.Count(x => string.Equals(x.Name, "MockFileSystemTests", StringComparison.Ordinal));

		await That(mockFileSystemTestsCount).IsGreaterThan(100);
	}
}
