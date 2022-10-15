﻿using System.Threading;

namespace Testably.Abstractions.Testing.Tests.Notification;

public class NotificationTests
{
	[Fact]
	public void AwaitableCallback_Amount_ShouldOnlyReturnAfterNumberOfCallbacks()
	{
		Testing.TimeSystemMock timeSystem = new();
		int receivedCount = 0;
		Testing.Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(t =>
			{
				if (t.TotalMilliseconds > 0)
				{
					receivedCount++;
				}
			});

		new Thread(() =>
		{
			Thread.Sleep(10);
			for (int i = 1; i <= 10; i++)
			{
				timeSystem.Thread.Sleep(i);
				Thread.Sleep(1);
			}
		}).Start();

		wait.Wait(count: 7);
		receivedCount.Should().BeGreaterOrEqualTo(7);
	}

	[Fact]
	public void AwaitableCallback_Filter_ShouldOnlyUpdateAfterFilteredValue()
	{
		Testing.TimeSystemMock timeSystem = new();
		int receivedCount = 0;
		Testing.Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				receivedCount++;
			});

		new Thread(() =>
		{
			Thread.Sleep(10);
			for (int i = 1; i <= 10; i++)
			{
				timeSystem.Thread.Sleep(i);
				Thread.Sleep(1);
			}
		}).Start();

		wait.Wait(t => t.TotalMilliseconds > 6);
		receivedCount.Should().BeGreaterOrEqualTo(6);
	}

	[Fact]
	public void AwaitableCallback_Predicate_ShouldOnlyUpdateAfterFilteredValue()
	{
		Testing.TimeSystemMock timeSystem = new();
		int receivedCount = 0;
		ManualResetEventSlim ms = new();
		timeSystem.On.ThreadSleep(_ =>
		{
			receivedCount++;
		}, t => t.TotalMilliseconds > 6);

		new Thread(() =>
		{
			Thread.Sleep(10);
			for (int i = 1; i <= 10; i++)
			{
				timeSystem.Thread.Sleep(i);
				Thread.Sleep(1);
			}

			ms.Set();
		}).Start();

		ms.Wait(30000);
		receivedCount.Should().BeLessOrEqualTo(4);
	}

	[Fact]
	public void AwaitableCallback_ShouldWaitForCallbackExecution()
	{
		ManualResetEventSlim ms = new();
		try
		{
			Testing.TimeSystemMock timeSystem = new();
			bool isCalled = false;
			Testing.Notification.IAwaitableCallback<TimeSpan> wait =
				timeSystem.On.ThreadSleep(_ =>
				{
					isCalled = true;
				});

			new Thread(() =>
			{
				while (!ms.IsSet)
				{
					timeSystem.Thread.Sleep(1);
					Thread.Sleep(1);
				}
			}).Start();

			wait.Wait();
			isCalled.Should().BeTrue();
		}
		finally
		{
			ms.Set();
		}
	}

	[Fact]
	public void AwaitableCallback_TimeoutExpired_ShouldThrowTimeoutException()
	{
		Testing.TimeSystemMock timeSystem = new();
		bool isCalled = false;
		Testing.Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				isCalled = true;
			});
		new Thread(() =>
		{
			// Delay larger than timeout of 10ms
			Thread.Sleep(1000);
			timeSystem.Thread.Sleep(1);
		}).Start();

		Exception? exception = Record.Exception(() =>
		{
			wait.Wait(timeout: 10);
		});

		exception.Should().BeOfType<TimeoutException>();
		isCalled.Should().BeFalse();
	}

	[Fact]
	public void AwaitableCallback_WaitedPreviously_ShouldWaitAgainForCallbackExecution()
	{
		int secondThreadMilliseconds = 42;
		int firstThreadMilliseconds = secondThreadMilliseconds + 1;
		ManualResetEventSlim ms = new();
		Testing.TimeSystemMock timeSystem = new();
		bool isCalledFromSecondThread = false;
		ManualResetEventSlim listening = new();
		Testing.Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On
			   .ThreadSleep(t =>
				{
					if (t.TotalMilliseconds.Equals(secondThreadMilliseconds))
					{
						isCalledFromSecondThread = true;
					}
				}).Execute(() => listening.Set());
		new Thread(() =>
		{
			listening.Wait(1000);
			timeSystem.Thread.Sleep(firstThreadMilliseconds);
		}).Start();
		wait.Wait();
		listening.Reset();

		new Thread(() =>
		{
			listening.Wait(1000);
			if (!ms.IsSet)
			{
				// Should only trigger, if the second call to `Wait` still blocks
				timeSystem.Thread.Sleep(secondThreadMilliseconds);
			}
		}).Start();

		wait.Wait();
		ms.Set();
		isCalledFromSecondThread.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Execute_ShouldBeExecutedBeforeWait(int milliseconds)
	{
		Testing.TimeSystemMock timeSystem = new();
		int receivedMilliseconds = -1;
		bool isExecuted = false;

		timeSystem.On.ThreadSleep(t =>
			{
				receivedMilliseconds = (int)t.TotalMilliseconds;
			}).Execute(() =>
			{
				timeSystem.Thread.Sleep(milliseconds);
			})
		   .Wait(executeWhenWaiting: () =>
			{
				isExecuted = true;
			});

		receivedMilliseconds.Should().Be(milliseconds);
		isExecuted.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Execute_WithReturnValue_ShouldBeExecutedAndReturnValue(
		int milliseconds, string result)
	{
		Testing.TimeSystemMock timeSystem = new();
		int receivedMilliseconds = -1;
		bool isExecuted = false;

		string actualResult = timeSystem.On.ThreadSleep(t =>
			{
				receivedMilliseconds = (int)t.TotalMilliseconds;
			}).Execute(() =>
			{
				timeSystem.Thread.Sleep(milliseconds);
				return result;
			})
		   .Wait(executeWhenWaiting: () =>
			{
				isExecuted = true;
			});

		receivedMilliseconds.Should().Be(milliseconds);
		actualResult.Should().Be(result);
		isExecuted.Should().BeTrue();
	}
}