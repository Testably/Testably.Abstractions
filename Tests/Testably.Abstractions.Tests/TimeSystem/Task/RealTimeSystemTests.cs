using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.TimeSystem.Task;

public static class RealTimeSystemTests
{
	public sealed class TaskTests : TimeSystemTaskTests<Abstractions.TimeSystem>
	{
		public TaskTests() : base(new Abstractions.TimeSystem())
		{
		}

		[SkippableFact]
		public async System.Threading.Tasks.Task
			Delay_Milliseconds_Canceled_ShouldDelayForSpecifiedMilliseconds()
		{
			int millisecondsTimeout = 100;
			System.DateTime before = TimeSystem.DateTime.UtcNow;
			CancellationTokenSource cts = new(millisecondsTimeout);
			CancellationToken cancellationToken = cts.Token;

			Exception? exception = await Record.ExceptionAsync(async () =>
					await TimeSystem.Task
					   .Delay(100 * millisecondsTimeout, cancellationToken))
			   .ConfigureAwait(false);
			System.DateTime after = TimeSystem.DateTime.UtcNow;

			after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout)
			   .ApplySystemClockTolerance());
			exception.Should().BeOfType<TaskCanceledException>();
		}

		[SkippableFact]
		public async System.Threading.Tasks.Task
			Delay_Timespan_Canceled_ShouldDelayForSpecifiedMilliseconds()
		{
			TimeSpan timeout = TimeSpan.FromMilliseconds(100);
			TimeSpan delay = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds * 100);
			System.DateTime before = TimeSystem.DateTime.UtcNow;
			CancellationTokenSource cts = new(timeout);
			CancellationToken cancellationToken = cts.Token;

			Exception? exception = await Record.ExceptionAsync(async () =>
			{
				await TimeSystem.Task
				   .Delay(delay, cancellationToken)
				   .ConfigureAwait(false);
			}).ConfigureAwait(false);
			System.DateTime after = TimeSystem.DateTime.UtcNow;

			after.Should().BeOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
			exception.Should().BeOfType<TaskCanceledException>();
		}
	}
}