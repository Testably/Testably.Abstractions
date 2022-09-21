using FluentAssertions;
using System;
using System.Threading;
using Testably.Abstractions.Testing;

namespace Examples.Timer;

public class TimerExampleTest
{
    [ExampleTest]
    public void Exception_ShouldBeHandled()
    {
        Exception thrownException = new("foo");
        FakeSynchronization synchronizationMock = new(() => throw thrownException);
        TimeSystemMock timeSystem = new();
        Exception? receivedException = null;
        CancellationTokenSource cancellationTokenSource = new(1000);
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        using TimerExample sut = new(
            timeSystem,
            synchronizationMock,
            ex =>
            {
                receivedException = ex;
                cancellationTokenSource.Cancel();
            });

        sut.Start(cancellationToken);

        cancellationToken.WaitHandle.WaitOne();
        receivedException.Should().Be(thrownException);
    }

    [ExampleTest]
    public void Execute_ShouldBeCalledRepeatedly()
    {
        int iterationCount = 5;
        CancellationTokenSource cancellationTokenSource = new(1000);
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        FakeSynchronization synchronizationMock = new(() =>
        {
            if (--iterationCount == 0)
            {
                cancellationTokenSource.Cancel();
            }
        });
        TimeSystemMock timeSystem = new();
        using TimerExample sut = new(
            timeSystem,
            synchronizationMock);

        sut.Start(cancellationToken);

        cancellationToken.WaitHandle.WaitOne();
        synchronizationMock.ExecutionCount.Should().BeGreaterOrEqualTo(5);
    }

    public class FakeSynchronization : TimerExample.ISynchronization
    {
        public int ExecutionCount { get; private set; }

        private readonly Action _callback;

        public FakeSynchronization(Action callback)
        {
            _callback = callback;
        }

        #region ISynchronization Members

        /// <inheritdoc />
        public TimeSpan Interval { get; } = TimeSpan.FromSeconds(1);

        /// <inheritdoc />
        public void Execute(CancellationToken cancellationToken)
        {
            ExecutionCount++;
            _callback.Invoke();
        }

        #endregion
    }
}