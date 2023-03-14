using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.Tests;

public class NotificationTests
{
	[SkippableFact]
	public void AwaitableCallback_Amount_ShouldOnlyReturnAfterNumberOfCallbacks()
	{
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

		Task.Run(async () =>
		{
			await Task.Delay(10);
			for (int i = 1; i <= 10; i++)
			{
				await timeSystem.Task.Delay(i);
				await Task.Delay(1);
			}
		});

		wait.Wait(count: 7);
		receivedCount.Should().BeGreaterOrEqualTo(7);
	}

	[SkippableFact]
	public async Task AwaitableCallback_Dispose_ShouldStopListening()
	{
		MockTimeSystem timeSystem = new();
		bool isCalled = false;
		Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				isCalled = true;
			});

		wait.Dispose();

		await timeSystem.Task.Delay(1);
		await Task.Delay(10);
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
		MockTimeSystem timeSystem = new();
		int receivedCount = 0;
		Notification.IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				receivedCount++;
			});

		Task.Run(async () =>
		{
			await Task.Delay(10);
			for (int i = 1; i <= 10; i++)
			{
				timeSystem.Thread.Sleep(i);
				await Task.Delay(1);
			}
		});

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
		
		Task.Run(async () =>
		{
			await Task.Delay(10);
			for (int i = 1; i <= 10; i++)
			{
				timeSystem.Thread.Sleep(i);
				await Task.Delay(1);
			}
			ms.Set();
		});

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

			Task.Run(async () =>
			{
				while (!ms.IsSet)
				{
					timeSystem.Thread.Sleep(1);
					await Task.Delay(1);
				}
			});

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

		Task.Run(() =>
		{
			// Delay larger than timeout of 10ms
			ms.Wait();
			timeSystem.Thread.Sleep(1);
		});

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
		Task.Run(() =>
		{
			listening.Wait(1000);
			timeSystem.Thread.Sleep(firstThreadMilliseconds);
		});
		wait.Wait();
		listening.Reset();

		Task.Run(() =>
		{
			listening.Wait(1000);
			if (!ms.IsSet)
			{
				// Should only trigger, if the second call to `Wait` still blocks
				timeSystem.Thread.Sleep(secondThreadMilliseconds);
			}
		});

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
