using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Testably.Abstractions.Tests.Time;

public partial class TimeSystemTests
{
    public class TaskTests
    {
        [Fact]
        public async Task
            Delay_Milliseconds_Canceled_ShouldDelayForSpecifiedMilliseconds()
        {
            TimeSystem timeSystem = new();
            int millisecondsTimeout = 100;
            DateTime before = DateTime.UtcNow;
            CancellationTokenSource
                cts = new(millisecondsTimeout);
            CancellationToken cancellationToken = cts.Token;

            Exception? exception = await Record.ExceptionAsync(async () =>
                    await timeSystem.Task
                       .Delay(10 * millisecondsTimeout, cancellationToken))
               .ConfigureAwait(false);
            DateTime after = DateTime.UtcNow;

            after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout).ApplySystemClockTolerance());
            exception.Should().BeAssignableTo<TaskCanceledException>();
        }

        [Fact]
        public async Task
            Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
        {
            TimeSystem timeSystem = new();

            Exception? exception = await Record.ExceptionAsync(async () =>
                {
                    await timeSystem.Task.Delay(-2).ConfigureAwait(false);
                }).ConfigureAwait(false);

            exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task Delay_Milliseconds_ShouldDelayForSpecifiedMilliseconds()
        {
            TimeSystem timeSystem = new();
            int millisecondsTimeout = 100;

            DateTime before = DateTime.UtcNow;
            await timeSystem.Task.Delay(millisecondsTimeout).ConfigureAwait(false);
            DateTime after = DateTime.UtcNow;

            after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout).ApplySystemClockTolerance());
        }

        [Fact]
        public async Task Delay_Timespan_Canceled_ShouldDelayForSpecifiedMilliseconds()
        {
            TimeSystem timeSystem = new();
            TimeSpan timeout = TimeSpan.FromMilliseconds(100);
            TimeSpan delay = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds * 100);
            DateTime before = DateTime.UtcNow;
            CancellationTokenSource cts = new(timeout);
            CancellationToken cancellationToken = cts.Token;

            Exception? exception = await Record.ExceptionAsync(async () =>
            {
                await timeSystem.Task
                   .Delay(delay, cancellationToken)
                   .ConfigureAwait(false);
            }).ConfigureAwait(false);
            DateTime after = DateTime.UtcNow;

            after.Should().BeOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
            exception.Should().BeAssignableTo<TaskCanceledException>();
        }

        [Fact]
        public async Task
            Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
        {
            TimeSystem timeSystem = new();

            Exception? exception = await Record.ExceptionAsync(async () =>
                {
                    await timeSystem.Task
                       .Delay(TimeSpan.FromMilliseconds(-2))
                       .ConfigureAwait(false);
                }).ConfigureAwait(false);

            exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task Delay_Timespan_ShouldDelayForSpecifiedMilliseconds()
        {
            TimeSystem timeSystem = new();
            TimeSpan timeout = TimeSpan.FromMilliseconds(100);

            DateTime before = DateTime.UtcNow;
            await timeSystem.Task.Delay(timeout).ConfigureAwait(false);
            DateTime after = DateTime.UtcNow;

            after.Should().BeOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
        }
    }
}