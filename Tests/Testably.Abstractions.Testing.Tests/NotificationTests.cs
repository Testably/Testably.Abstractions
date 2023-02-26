using System.Threading;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests;

public class NotificationTests
{
	[SkippableFact]
	public void AwaitableCallback_Amount_ShouldOnlyReturnAfterNumberOfCallbacks()
	{
		Skip.If(Test.RunsOnWindows, "Brittle test under Windows on GitHub");

		MockTimeSystem timeSystem = new();
		int receivedCount = 0;
		Notification.IAwaitableCallback<TimeSpan> wait =
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

	[SkippableFact]
	public void AwaitableCallback_Dispose_ShouldStopListening()
	{
		MockTimeSystem timeSystem = new();
		bool isCalled = false;
		Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				isCalled = true;
			});

		wait.Dispose();

		timeSystem.Thread.Sleep(1);
		Thread.Sleep(10);
		isCalled.Should().BeFalse();
	}

	[SkippableFact]
	public void AwaitableCallback_DisposeFromExecuteWhileWaiting_ShouldStopListening()
	{
		MockTimeSystem timeSystem = new();
		bool isCalled = false;
		Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On
				.ThreadSleep(_ =>
				{
					isCalled = true;
				})
				.ExecuteWhileWaiting(() => { });

		wait.Dispose();

		timeSystem.Thread.Sleep(1);
		Thread.Sleep(10);
		isCalled.Should().BeFalse();
	}

	[SkippableFact]
	public void AwaitableCallback_Filter_ShouldOnlyUpdateAfterFilteredValue()
	{
		Skip.If(Test.RunsOnWindows, "Brittle test under Windows on GitHub");

		MockTimeSystem timeSystem = new();
		int receivedCount = 0;
		Notification.IAwaitableCallback<TimeSpan> wait =
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

	[SkippableFact]
	public void AwaitableCallback_Predicate_ShouldOnlyUpdateAfterFilteredValue()
	{
		MockTimeSystem timeSystem = new();
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

	[SkippableFact]
	public void AwaitableCallback_ShouldWaitForCallbackExecution()
	{
		ManualResetEventSlim ms = new();
		try
		{
			MockTimeSystem timeSystem = new();
			bool isCalled = false;
			Notification.IAwaitableCallback<TimeSpan> wait =
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

	[SkippableFact]
	public void AwaitableCallback_TimeoutExpired_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new();
		bool isCalled = false;
		ManualResetEventSlim ms = new();
		Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				isCalled = true;
			});
		new Thread(() =>
		{
			// Delay larger than timeout of 10ms
			ms.Wait();
			timeSystem.Thread.Sleep(1);
		}).Start();

		Exception? exception = Record.Exception(() =>
		{
			wait.Wait(timeout: 10);
		});

		exception.Should().BeOfType<TimeoutException>();
		isCalled.Should().BeFalse();
		ms.Set();
	}

	[SkippableFact]
	public void AwaitableCallback_WaitedPreviously_ShouldWaitAgainForCallbackExecution()
	{
		int secondThreadMilliseconds = 42;
		int firstThreadMilliseconds = secondThreadMilliseconds + 1;
		ManualResetEventSlim ms = new();
		MockTimeSystem timeSystem = new();
		bool isCalledFromSecondThread = false;
		ManualResetEventSlim listening = new();
		Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On
				.ThreadSleep(t =>
				{
					if (t.TotalMilliseconds.Equals(secondThreadMilliseconds))
					{
						isCalledFromSecondThread = true;
					}
				}).ExecuteWhileWaiting(() => listening.Set());
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
		MockTimeSystem timeSystem = new();
		int receivedMilliseconds = -1;
		bool isExecuted = false;

		timeSystem.On
			.ThreadSleep(t =>
			{
				receivedMilliseconds = (int)t.TotalMilliseconds;
			})
			.ExecuteWhileWaiting(() =>
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
		MockTimeSystem timeSystem = new();
		int receivedMilliseconds = -1;
		bool isExecuted = false;

		string actualResult = timeSystem.On
			.ThreadSleep(t =>
			{
				receivedMilliseconds = (int)t.TotalMilliseconds;
			})
			.ExecuteWhileWaiting(() =>
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
