namespace Testably.Abstractions.Testing.Tests;

public class MockRandomSystemTests
{
	[Fact]
	public async Task ToString_ShouldContainMockRandomSystem()
	{
		MockRandomSystem randomSystem = new();

		string result = randomSystem.ToString();

		await That(result).Contains(nameof(MockRandomSystem));
	}
}
