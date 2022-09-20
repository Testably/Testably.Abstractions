﻿using System.Threading;

namespace Testably.Abstractions.Tests.Testing;

public class NotificationTests
{
    [Fact]
    public void AwaitableCallback_Amount_ShouldOnlyReturnAfterNumberOfCallbacks()
    {
        TimeSystemMock timeSystem = new();
        int totalCount = 0;
        int filteredCount = 0;
        Notification.IAwaitableCallback<TimeSpan> wait = timeSystem.On.ThreadSleep(_ =>
        {
            totalCount++;
        });
        new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                timeSystem.Thread.Sleep(10 * i);
            }
        }).Start();

        bool result = wait.Wait(_ =>
        {
            filteredCount++;
            return true;
        }, count: 6);

        filteredCount.Should().BeGreaterOrEqualTo(totalCount - 1).And
           .BeLessOrEqualTo(totalCount);
        result.Should().BeTrue();
        totalCount.Should().BeGreaterOrEqualTo(6);
    }

    [Fact]
    public void AwaitableCallback_Filter_ShouldOnlyUpdateAfterFilteredValue()
    {
        TimeSystemMock timeSystem = new();
        int totalCount = 0;
        int filteredCount = 0;
        Notification.IAwaitableCallback<TimeSpan> wait = timeSystem.On.ThreadSleep(_ =>
        {
            totalCount++;
        });
        new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                timeSystem.Thread.Sleep(10 * i);
            }
        }).Start();

        bool result = wait.Wait(t =>
        {
            if (t.TotalMilliseconds > 60)
            {
                filteredCount++;
                return true;
            }

            return false;
        });

        filteredCount.Should().BeLessThan(totalCount);
        result.Should().BeTrue();
        totalCount.Should().BeGreaterOrEqualTo(6);
    }

    [Fact]
    public void AwaitableCallback_ShouldWaitForCallbackExecution()
    {
        TimeSystemMock timeSystem = new();
        bool isCalled = false;
        Notification.IAwaitableCallback<DateTime> wait = timeSystem.On.DateTimeRead(_ =>
        {
            isCalled = true;
        });
        new Thread(() =>
        {
            Thread.Sleep(10);
            _ = timeSystem.DateTime.Now;
        }).Start();

        bool result = wait.Wait();

        result.Should().BeTrue();
        isCalled.Should().BeTrue();
    }

    [Fact]
    public void AwaitableCallback_TimeoutExpired_ShouldStopAfterTimeout()
    {
        TimeSystemMock timeSystem = new();
        bool isCalled = false;
        Notification.IAwaitableCallback<DateTime> wait = timeSystem.On.DateTimeRead(_ =>
        {
            isCalled = true;
        });
        new Thread(() =>
        {
            Thread.Sleep(1000);
            _ = timeSystem.DateTime.Now;
        }).Start();

        bool result = wait.Wait(timeout: 10);

        result.Should().BeFalse();
        isCalled.Should().BeFalse();
    }
}