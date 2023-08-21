namespace Testably.Abstractions.Testing.Tests;

public class MockRandomSystemTests
{
	[Fact]
	public void ToString_ShouldContainMockRandomSystem()
	{
		MockRandomSystem randomSystem = new();

		string result = randomSystem.ToString();

		result.Should().Contain(nameof(MockRandomSystem));
	}
}
