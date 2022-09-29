using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(RealTimeSystem))]
    public sealed class TaskTests : TimeSystemTaskTests<TimeSystem>
    {
        #region Test Setup

        public TaskTests() : base(new TimeSystem())
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