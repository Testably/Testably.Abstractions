using FluentAssertions;
using System;
using System.Threading;
using Testably.Abstractions.Testing;

namespace Examples.Timer;

/// <summary>
///     The <see cref="TimerExampleTest" /> uses the <see cref="TimeSystemMock" /> to mock the underlying calls to
///     <see cref="System.Threading.Tasks.Task.Delay(TimeSpan)" />, so that the functionality can be tested without
///     actually blocking for the required <see cref="SynchronizationMock.Interval" />, so the unit tests execute in a
///     matter of milliseconds.
/// </summary>
public class TimerExampleTest
{
    /// <summary>
    ///     Tests that the `thrownException` is caught and forwarded to the
    ///     errorHandler of the <see cref="TimerExample" /> constructor parameter.
    /// </summary>
    [ExampleTest]
    public void Exception_ShouldBeHandled()
    {
        Exception thrownException = new("foo");
        SynchronizationMock synchronizationMockMock = new(() =>
        {
            // Add a delay (e.g. `Thread.Sleep(2000);`) to fail the test due to the fallback timeout.
            throw thrownException;
        });
        TimeSystemMock timeSystem = new();
        Exception? receivedException = null;
        // Add a fallback timeout that will fail the test latest after 1 second
        CancellationTokenSource cancellationTokenSource = new(1000);
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        using TimerExample sut = new(
            timeSystem,
            synchronizationMockMock,
            ex =>
            {
                receivedException = ex;
                cancellationTokenSource.Cancel();
            });

        sut.Start(cancellationToken);

        cancellationToken.WaitHandle.WaitOne();
        receivedException.Should().Be(thrownException);
    }

    /// <summary>
    ///     Tests that the callback is executed repeatedly (at least `iterationCount` times).
    /// </summary>
    [ExampleTest]
    public void Execute_ShouldBeCalledRepeatedly()
    {
        int iterationCount = 5;
        // Add a fallback timeout that will fail the test latest after 1 second
        CancellationTokenSource cancellationTokenSource = new(1000);
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        SynchronizationMock synchronizationMockMock = new(() =>
        {
            // Add a delay (e.g. `Thread.Sleep(2000);`) to fail the test due to the fallback timeout.
            if (--iterationCount == 0)
            {
                cancellationTokenSource.Cancel();
            }
        });
        TimeSystemMock timeSystem = new();
        using TimerExample sut = new(
            timeSystem,
            synchronizationMockMock);

        sut.Start(cancellationToken);

        cancellationToken.WaitHandle.WaitOne();
        synchronizationMockMock.ExecutionCount.Should().BeGreaterOrEqualTo(5);
    }

    /// <summary>
    ///     Helper class to mock the <see cref="TimerExample.ISynchronization" /> for testing purposes.
    /// </summary>
    public class SynchronizationMock : TimerExample.ISynchronization
    {
        /// <summary>
        ///     Counts the number of executions of the callback.
        /// </summary>
        public int ExecutionCount { get; private set; }

        private readonly Action _callback;

        /// <summary>
        ///     Initializes a new instance of <see cref="SynchronizationMock" /> that invokes
        ///     the <paramref name="callback" /> in the <see cref="Execute" /> method.
        /// </summary>
        public SynchronizationMock(Action callback)
        {
            _callback = callback;
        }

        #region ISynchronization Members

        /// <inheritdoc cref="TimerExample.ISynchronization.Interval" />
        public TimeSpan Interval { get; } = TimeSpan.FromSeconds(10);

        /// <inheritdoc cref="TimerExample.ISynchronization.Execute(CancellationToken)" />
        public void Execute(CancellationToken cancellationToken)
        {
            ExecutionCount++;
            _callback.Invoke();
        }

        #endregion
    }
}