using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.TimeSystem.Task;

public static class MockTimeSystemTests
{
	public sealed class TaskTests : TimeSystemTaskTests<MockTimeSystem>
	{
		#region Test Setup

		public TaskTests() : base(new MockTimeSystem())
		{
		}

		#endregion

		[Fact]
		public async System.Threading.Tasks.Task
			Delay_Milliseconds_Canceled_ShouldDelayForSpecifiedMilliseconds()
		{
			CancellationTokenSource cts = new();
			cts.Cancel();
			CancellationToken cancellationToken = cts.Token;

			System.DateTime before = TimeSystem.DateTime.UtcNow;
			Exception? exception = await Record.ExceptionAsync(async () =>
				await TimeSystem.Task.Delay(1000, cancellationToken));
			System.DateTime after = TimeSystem.DateTime.UtcNow;

			after.Should().Be(before);
			exception.Should().BeOfType<TaskCanceledException>();
		}

		[Fact]
		public async System.Threading.Tasks.Task
			Delay_Timespan_Canceled_ShouldDelayForSpecifiedMilliseconds()
		{
			CancellationTokenSource cts = new();
			cts.Cancel();
			CancellationToken cancellationToken = cts.Token;

			System.DateTime before = TimeSystem.DateTime.UtcNow;
			Exception? exception = await Record.ExceptionAsync(async () =>
				await TimeSystem.Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));
			System.DateTime after = TimeSystem.DateTime.UtcNow;

			after.Should().Be(before);
			exception.Should().BeOfType<TaskCanceledException>();
		}
	}
}