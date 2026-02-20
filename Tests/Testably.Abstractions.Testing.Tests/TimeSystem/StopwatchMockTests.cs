using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class StopwatchMockTests
{
	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(15)]
	[InlineData(123456789000)]
	public async Task ShouldSupportTicksPrecision(long delayTicks)
	{
		MockTimeSystem timeSystem = new();
		IStopwatch stopwatch = timeSystem.Stopwatch.StartNew();
		await timeSystem.Task.Delay(TimeSpan.FromTicks(delayTicks),
			TestContext.Current.CancellationToken);

		long elapsedTicks = stopwatch.ElapsedTicks;

		await That(elapsedTicks).IsEqualTo(delayTicks);
	}
}
