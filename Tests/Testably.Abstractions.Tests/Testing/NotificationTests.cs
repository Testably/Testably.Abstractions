﻿using System.Threading;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Testing;

public class NotificationTests
{
    #region Test Setup

    private readonly ITestOutputHelper _testOutputHelper;

    public NotificationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    #endregion

    [Fact]
    [Trait(nameof(Testing), nameof(Notification))]
    public void AwaitableCallback_Amount_ShouldOnlyReturnAfterNumberOfCallbacks()
    {
        TimeSystemMock timeSystem = new();
        int totalCount = 0;
        int filteredCount = 0;
        ManualResetEventSlim ms = new();
        Notification.IAwaitableCallback<TimeSpan> wait = timeSystem.On.ThreadSleep(_ =>
        {
            totalCount++;
        });
        new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                timeSystem.Thread.Sleep(0);
                if (ms.Wait(20))
                {
                    break;
                }
            }

            for (int i = 0; i < 10; i++)
            {
                timeSystem.Thread.Sleep(10 * i);
            }
        }).Start();

        wait.Wait(t =>
        {
            if (t.TotalMilliseconds == 0)
            {
                ms.Set();
            }

            filteredCount++;
            return true;
        }, count: 6);

        filteredCount.Should().BeGreaterOrEqualTo(totalCount - 1).And
           .BeLessOrEqualTo(totalCount);
        totalCount.Should().BeGreaterOrEqualTo(6);
    }

    [SkippableFact]
    [Trait(nameof(Testing), nameof(Notification))]
    public void AwaitableCallback_Filter_ShouldOnlyUpdateAfterFilteredValue()
    {
        Skip.IfNot(Test.RunsOnWindows, "Test is brittle, especially on MacOS.");

        TimeSystemMock timeSystem = new();
        int totalCount = 0;
        int filteredCount = 0;
        ManualResetEventSlim ms = new();
        Notification.IAwaitableCallback<TimeSpan> wait = timeSystem.On.ThreadSleep(_ =>
        {
            totalCount++;
        });
        new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                timeSystem.Thread.Sleep(0);
                if (ms.Wait(20))
                {
                    break;
                }
            }

            for (int i = 0; i < 10; i++)
            {
                _testOutputHelper.WriteLine($"Trigger Thread.Sleep for {10 * i}ms...");
                timeSystem.Thread.Sleep(10 * i);
            }
        }).Start();

        wait.Wait(t =>
        {
            if (t.TotalMilliseconds == 0)
            {
                ms.Set();
            }

            if (t.TotalMilliseconds > 60)
            {
                filteredCount++;
                _testOutputHelper.WriteLine(
                    $"  Filter for {t} > 60ms: matched ({filteredCount} times)");
                return true;
            }

            _testOutputHelper.WriteLine(
                $"  Filter for {t} > 60ms : no match ({filteredCount} times)");

            return false;
        });

        filteredCount.Should().BeLessThan(totalCount);
        totalCount.Should().BeGreaterOrEqualTo(6);
    }

    [SkippableFact]
    [Trait(nameof(Testing), nameof(Notification))]
    public void AwaitableCallback_ShouldWaitForCallbackExecution()
    {
        Skip.IfNot(Test.RunsOnWindows, "Test is brittle, especially on MacOS.");
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

        wait.Wait();

        isCalled.Should().BeTrue();
    }

    [Fact]
    [Trait(nameof(Testing), nameof(Notification))]
    public void AwaitableCallback_TimeoutExpired_ShouldThrowTimeoutException()
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

        Exception? exception = Record.Exception(() =>
        {
            wait.Wait(timeout: 10);
        });

        exception.Should().BeOfType<TimeoutException>();
        isCalled.Should().BeFalse();
    }
}