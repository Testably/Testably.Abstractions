using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.Time;

public partial class TimeSystemMockTests
{
    public class TaskTests
    {
        [Fact]
        public async Task
            Delay_Milliseconds_Canceled_ShouldDelayForSpecifiedMilliseconds()
        {
            TimeSystemMock timeSystem = new();
            CancellationTokenSource cts = new();
            cts.Cancel();
            CancellationToken cancellationToken = cts.Token;

            DateTime before = timeSystem.DateTime.UtcNow;
            Exception? exception = await Record.ExceptionAsync(async () =>
                await timeSystem.Task.Delay(1000, cancellationToken));
            DateTime after = timeSystem.DateTime.UtcNow;

            after.Should().Be(before);
            exception.Should().BeAssignableTo<TaskCanceledException>();
        }

        [Fact]
        public async Task
            Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
        {
            TimeSystemMock timeSystem = new();

            Exception? exception =
                await Record.ExceptionAsync(() => timeSystem.Task.Delay(-2));

            exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task
            Delay_Milliseconds_ShouldDelayForSpecifiedMilliseconds()
        {
            DateTime now = Helpers.GetRandomTime(DateTimeKind.Utc);
            TimeSystemMock timeSystem = new(now);
            int millisecondsTimeout = 10000;

            await timeSystem.Task.Delay(millisecondsTimeout);
            DateTime after = timeSystem.DateTime.UtcNow;

            after.Should().Be(now.AddMilliseconds(millisecondsTimeout));
        }

        [Fact]
        public async Task
            Delay_Timespan_Canceled_ShouldDelayForSpecifiedMilliseconds()
        {
            TimeSystemMock timeSystem = new();
            CancellationTokenSource cts = new();
            cts.Cancel();
            CancellationToken cancellationToken = cts.Token;

            DateTime before = timeSystem.DateTime.UtcNow;
            Exception? exception = await Record.ExceptionAsync(async () =>
                await timeSystem.Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));
            DateTime after = timeSystem.DateTime.UtcNow;

            after.Should().Be(before);
            exception.Should().BeAssignableTo<TaskCanceledException>();
        }

        [Fact]
        public async Task
            Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
        {
            TimeSystemMock timeSystem = new();

            Exception? exception = await Record.ExceptionAsync(() =>
                timeSystem.Task.Delay(TimeSpan.FromMilliseconds(-2)));

            exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task
            Delay_Timespan_ShouldDelayForSpecifiedMilliseconds()
        {
            DateTime now = Helpers.GetRandomTime(DateTimeKind.Utc);
            TimeSystemMock timeSystem = new(now);
            TimeSpan timeout = TimeSpan.FromMinutes(1);

            await timeSystem.Task.Delay(timeout);
            DateTime after = timeSystem.DateTime.UtcNow;

            after.Should().Be(now.Add(timeout));
        }
    }
}