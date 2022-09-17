using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Extensions.Tests;

public class TimeSystemExtensionsTimerTests
{
    [Fact]
    public void CreateTimer_Dispose_ShouldStopIterations()
    {
        TimeSystemMock timeSystem = Initialize(
            out TimeSpan interval,
            out int expectedIterations,
            out TimeSpan[] receivedIntervals);
        timeSystem.On.TaskDelay(d =>
            receivedIntervals[Math.Max(0, expectedIterations)] = d);
        IRunningTimer? runningTimer = null;
        ManualResetEventSlim ms = new();

        IStoppedTimer timer = timeSystem.CreateTimer(interval, _ =>
        {
            expectedIterations--;
            if (expectedIterations < 0)
            {
                // ReSharper disable once AccessToModifiedClosure
                runningTimer?.Dispose();
                ms.Set();
            }
        });
        runningTimer = timer.Start();
        ms.Wait(1000);

        receivedIntervals.Should().AllBeEquivalentTo(interval);
    }

    [Fact]
    public void CreateTimer_Exception_ShouldCallOnErrorAndContinueIterations()
    {
        Exception exception = new("foo");
        List<Exception> receivedExceptions = new();
        TimeSystemMock timeSystem = Initialize(
            out TimeSpan interval,
            out int expectedIterations,
            out TimeSpan[] receivedIntervals);
        timeSystem.On.TaskDelay(d =>
            receivedIntervals[Math.Max(0, expectedIterations)] = d);
        IRunningTimer? runningTimer = null;
        ManualResetEventSlim ms = new();

        IStoppedTimer timer = timeSystem.CreateTimer(interval, _ =>
        {
            expectedIterations--;
            if (expectedIterations % 3 == 0)
            {
                throw exception;
            }

            if (expectedIterations < 0)
            {
                // ReSharper disable once AccessToModifiedClosure
                runningTimer?.Dispose();
                ms.Set();
            }
        }, e => receivedExceptions.Add(e));
        runningTimer = timer.Start();
        ms.Wait(1000);

        receivedIntervals.Should().AllBeEquivalentTo(interval);
        receivedExceptions.Should().AllBeEquivalentTo(exception);
        receivedExceptions.Count.Should()
           .BeGreaterOrEqualTo(receivedIntervals.Length / 3);
    }

    [Fact]
    public void CreateTimer_InfiniteDelay_ShouldUseTimeSpanZero()
    {
        TimeSystemMock timeSystem = Initialize(
            out _,
            out int expectedIterations,
            out TimeSpan[] receivedIntervals);
        timeSystem.On.TaskDelay(d =>
            receivedIntervals[Math.Max(0, expectedIterations)] = d);
        CancellationTokenSource cancellationTokenSource = new();
        ManualResetEventSlim ms = new();

        IStoppedTimer timer = timeSystem.CreateTimer(Timeout.InfiniteTimeSpan, _ =>
        {
            expectedIterations--;
            if (expectedIterations < 0)
            {
                cancellationTokenSource.Cancel();
                ms.Set();
            }
        });
        timer.Start(cancellationTokenSource.Token);
        ms.Wait(1000);

        receivedIntervals.Should().AllBeEquivalentTo(TimeSpan.Zero);
    }

    [Fact]
    public void CreateTimer_Start_ShouldIterateUntilCancelled()
    {
        TimeSystemMock timeSystem = Initialize(
            out TimeSpan interval,
            out int expectedIterations,
            out TimeSpan[] receivedIntervals);
        timeSystem.On.TaskDelay(d =>
            receivedIntervals[Math.Max(0, expectedIterations)] = d);
        CancellationTokenSource cancellationTokenSource = new();
        ManualResetEventSlim ms = new();

        IStoppedTimer timer = timeSystem.CreateTimer(interval, _ =>
        {
            expectedIterations--;
            if (expectedIterations < 0)
            {
                cancellationTokenSource.Cancel();
                ms.Set();
            }
        });
        timer.Start(cancellationTokenSource.Token);
        ms.Wait(1000);

        receivedIntervals.Should().AllBeEquivalentTo(interval);
    }

    [Fact]
    public void CreateTimer_Start_ShouldIterateUntilStopped()
    {
        TimeSystemMock timeSystem = Initialize(
            out TimeSpan interval,
            out int expectedIterations,
            out TimeSpan[] receivedIntervals);
        timeSystem.On.TaskDelay(d =>
            receivedIntervals[Math.Max(0, expectedIterations)] = d);
        IRunningTimer? runningTimer = null;
        ManualResetEventSlim ms = new();

        IStoppedTimer timer = timeSystem.CreateTimer(interval, _ =>
        {
            expectedIterations--;
            if (expectedIterations < 0)
            {
                // ReSharper disable once AccessToModifiedClosure
                runningTimer?.Stop();
                ms.Set();
            }
        });
        runningTimer = timer.Start();
        ms.Wait(1000);

        receivedIntervals.Should().AllBeEquivalentTo(interval);
    }

    #region Helpers

    private static TimeSystemMock Initialize(
        out TimeSpan interval,
        out int expectedIterations,
        out TimeSpan[] receivedIntervals)
    {
        Random random = new Random();
        interval = TimeSpan.FromSeconds(random.Next(1, 100));
        expectedIterations = random.Next(5, 15);
        receivedIntervals = new TimeSpan[expectedIterations];
        return new TimeSystemMock();
    }

    #endregion
}