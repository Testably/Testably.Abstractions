using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract partial class TimeSystemTaskTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RealTimeSystem : TimeSystemTaskTests<TimeSystem>
    {
        #region Test Setup

        public RealTimeSystem() : base(new TimeSystem())
        {
        }

        #endregion

        [Fact]
        public async Task
            Delay_Milliseconds_Canceled_ShouldDelayForSpecifiedMilliseconds()
        {
            int millisecondsTimeout = 100;
            DateTime before = TimeSystem.DateTime.UtcNow;
            CancellationTokenSource cts = new(millisecondsTimeout);
            CancellationToken cancellationToken = cts.Token;

            Exception? exception = await Record.ExceptionAsync(async () =>
                    await TimeSystem.Task
                       .Delay(10 * millisecondsTimeout, cancellationToken))
               .ConfigureAwait(false);
            DateTime after = TimeSystem.DateTime.UtcNow;

            after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout)
               .ApplySystemClockTolerance());
            exception.Should().BeOfType<TaskCanceledException>();
        }

        [Fact]
        public async Task Delay_Timespan_Canceled_ShouldDelayForSpecifiedMilliseconds()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(100);
            TimeSpan delay = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds * 100);
            DateTime before = TimeSystem.DateTime.UtcNow;
            CancellationTokenSource cts = new(timeout);
            CancellationToken cancellationToken = cts.Token;

            Exception? exception = await Record.ExceptionAsync(async () =>
            {
                await TimeSystem.Task
                   .Delay(delay, cancellationToken)
                   .ConfigureAwait(false);
            }).ConfigureAwait(false);
            DateTime after = TimeSystem.DateTime.UtcNow;

            after.Should().BeOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
            exception.Should().BeOfType<TaskCanceledException>();
        }
    }
}