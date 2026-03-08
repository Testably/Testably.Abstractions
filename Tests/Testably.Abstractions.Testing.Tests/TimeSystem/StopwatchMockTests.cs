using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class StopwatchMockTests
{
	[Test]
	[Arguments(0)]
	[Arguments(1)]
	[Arguments(15)]
	[Arguments(1234567890)]
	public async Task ShouldSupportTicksPrecision(long delayTicks)
	{
		MockTimeSystem timeSystem = new();
		IStopwatch stopwatch = timeSystem.Stopwatch.StartNew();
		await timeSystem.Task.Delay(TimeSpan.FromTicks(delayTicks),
			TestContext.Current!.Execution.CancellationToken);

		long elapsedTicks = stopwatch.ElapsedTicks;

		long actualStopwatchTicks = elapsedTicks * TimeSpan.TicksPerSecond / timeSystem.Stopwatch.Frequency;
		await That(actualStopwatchTicks).IsEqualTo(delayTicks);
	}
}
