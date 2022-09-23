using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class TaskTests : TimeSystemTaskTests<TimeSystemMock>
    {
        #region Test Setup

        public TaskTests() : base(new TimeSystemMock())
        {
        }

        #endregion

        [Fact]
        public async Task
            Delay_Milliseconds_Canceled_ShouldDelayForSpecifiedMilliseconds()
        {
            CancellationTokenSource cts = new();
            cts.Cancel();
            CancellationToken cancellationToken = cts.Token;

            DateTime before = TimeSystem.DateTime.UtcNow;
            Exception? exception = await Record.ExceptionAsync(async () =>
                await TimeSystem.Task.Delay(1000, cancellationToken));
            DateTime after = TimeSystem.DateTime.UtcNow;

            after.Should().Be(before);
            exception.Should().BeOfType<TaskCanceledException>();
        }

        [Fact]
        public async Task
            Delay_Timespan_Canceled_ShouldDelayForSpecifiedMilliseconds()
        {
            CancellationTokenSource cts = new();
            cts.Cancel();
            CancellationToken cancellationToken = cts.Token;

            DateTime before = TimeSystem.DateTime.UtcNow;
            Exception? exception = await Record.ExceptionAsync(async () =>
                await TimeSystem.Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));
            DateTime after = TimeSystem.DateTime.UtcNow;

            after.Should().Be(before);
            exception.Should().BeOfType<TaskCanceledException>();
        }
    }
}