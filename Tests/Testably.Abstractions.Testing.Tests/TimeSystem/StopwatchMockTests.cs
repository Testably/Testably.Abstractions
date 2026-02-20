using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class StopwatchMockTests
{
	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(15)]
	[InlineData(1234567890)]
	public async Task ShouldSupportTicksPrecision(long delayTicks)
	{
		MockTimeSystem timeSystem = new();
		IStopwatch stopwatch = timeSystem.Stopwatch.StartNew();
		await timeSystem.Task.Delay(TimeSpan.FromTicks(delayTicks),
			TestContext.Current.CancellationToken);

		long elapsedTicks = stopwatch.ElapsedTicks;

		long actualStopwatchTicks = elapsedTicks * TimeSpan.TicksPerSecond / timeSystem.Stopwatch.Frequency;
		await That(actualStopwatchTicks).IsEqualTo(delayTicks);
	}
}
