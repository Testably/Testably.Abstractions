namespace Testably.Abstractions.Tests.TimeSystem;

public sealed class MockTimeSystemTests : TimeSystemTests<TimeSystemMock>
{
	public MockTimeSystemTests() : base(new TimeSystemMock())
	{
	}
}