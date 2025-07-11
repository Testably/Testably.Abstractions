﻿using System.Threading;

namespace Testably.Abstractions.Testing.Tests;

public class NotificationTests
{
	[Fact]
	public async Task AwaitableCallback_Amount_ShouldOnlyReturnAfterNumberOfCallbacks()
	{
		MockTimeSystem timeSystem = new();
		int receivedCount = 0;
		IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(t =>
			{
				if (t.TotalMilliseconds > 0)
				{
					receivedCount++;
				}
			});

		_ = Task.Run(async () =>
		{
			await Task.Delay(10, TestContext.Current.CancellationToken);
			for (int i = 1; i <= 10; i++)
			{
				timeSystem.Thread.Sleep(i);
				await Task.Delay(1, TestContext.Current.CancellationToken);
			}
		}, TestContext.Current.CancellationToken);

		wait.Wait(count: 7);
		await That(receivedCount).IsGreaterThanOrEqualTo(7);
	}

	[Fact]
	public async Task AwaitableCallback_Dispose_ShouldStopListening()
	{
		MockTimeSystem timeSystem = new();
		bool isCalled = false;
		IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				isCalled = true;
			});

		wait.Dispose();

		timeSystem.Thread.Sleep(1);
		await Task.Delay(10, TestContext.Current.CancellationToken);
		await That(isCalled).IsFalse();
	}

	[Fact]
	public async Task AwaitableCallback_DisposeFromExecuteWhileWaiting_ShouldStopListening()
	{
		MockTimeSystem timeSystem = new();
		bool isCalled = false;
		IAwaitableCallback<TimeSpan> wait =
			timeSystem.On
				.ThreadSleep(_ =>
				{
					isCalled = true;
				})
				.ExecuteWhileWaiting(() => { });

		wait.Dispose();

		timeSystem.Thread.Sleep(1);
		await Task.Delay(10, TestContext.Current.CancellationToken);
		await That(isCalled).IsFalse();
	}

	[Fact]
	public async Task AwaitableCallback_Filter_ShouldOnlyUpdateAfterFilteredValue()
	{
		MockTimeSystem timeSystem = new();
		int receivedCount = 0;
		IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				receivedCount++;
			});

		_ = Task.Run(async () =>
		{
			await Task.Delay(10, TestContext.Current.CancellationToken);
			for (int i = 1; i <= 10; i++)
			{
				timeSystem.Thread.Sleep(i);
				await Task.Delay(1, TestContext.Current.CancellationToken);
			}
		}, TestContext.Current.CancellationToken);

		wait.Wait(t => t.TotalMilliseconds > 6);
		await That(receivedCount).IsGreaterThanOrEqualTo(6);
	}

	[Fact]
	public async Task AwaitableCallback_Predicate_ShouldOnlyUpdateAfterFilteredValue()
	{
		MockTimeSystem timeSystem = new();
		int receivedCount = 0;
		using ManualResetEventSlim ms = new();
		timeSystem.On.ThreadSleep(_ =>
		{
			receivedCount++;
		}, t => t.TotalMilliseconds > 6);

		_ = Task.Run(async () =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				await Task.Delay(10, TestContext.Current.CancellationToken);
				for (int i = 1; i <= 10; i++)
				{
					timeSystem.Thread.Sleep(i);
					await Task.Delay(1, TestContext.Current.CancellationToken);
				}

				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}, TestContext.Current.CancellationToken);

		ms.Wait(30000, TestContext.Current.CancellationToken);
		await That(receivedCount).IsLessThanOrEqualTo(4);
	}

	[Fact]
	public async Task AwaitableCallback_ShouldWaitForCallbackExecution()
	{
		using ManualResetEventSlim ms = new();
		try
		{
			MockTimeSystem timeSystem = new();
			bool isCalled = false;
			IAwaitableCallback<TimeSpan> wait =
				timeSystem.On.ThreadSleep(_ =>
				{
					isCalled = true;
				});

			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						timeSystem.Thread.Sleep(1);
						await Task.Delay(1, TestContext.Current.CancellationToken);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, TestContext.Current.CancellationToken);

			wait.Wait();
			await That(isCalled).IsTrue();
		}
		finally
		{
			ms.Set();
		}
	}

	[Fact]
	public async Task AwaitableCallback_TimeoutExpired_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new();
		bool isCalled = false;
		using ManualResetEventSlim ms = new();
		IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep(_ =>
			{
				isCalled = true;
			});
		new Thread(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				// Delay larger than timeout of 10ms
				ms.Wait(TestContext.Current.CancellationToken);
				timeSystem.Thread.Sleep(1);
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}).Start();

		Exception? exception = Record.Exception(() =>
		{
			wait.Wait(timeout: 10);
		});

		await That(exception).IsExactly<TimeoutException>();
		await That(isCalled).IsFalse();
		ms.Set();
	}

	[Fact]
	public async Task AwaitableCallback_Wait_AfterDispose_ShouldThrowObjectDisposedException()
	{
		MockTimeSystem timeSystem = new();
		IAwaitableCallback<TimeSpan> wait =
			timeSystem.On.ThreadSleep();

		wait.Dispose();

		Exception? exception = Record.Exception(() =>
		{
			wait.Wait(timeout: 100);
		});

		await That(exception).IsExactly<ObjectDisposedException>();
	}

	[Fact]
	public async Task AwaitableCallback_WaitedPreviously_ShouldWaitAgainForCallbackExecution()
	{
		int secondThreadMilliseconds = 42;
		int firstThreadMilliseconds = secondThreadMilliseconds + 1;
		using ManualResetEventSlim ms = new();
		MockTimeSystem timeSystem = new();
		bool isCalledFromSecondThread = false;
		using ManualResetEventSlim listening = new();
		IAwaitableCallback<TimeSpan> wait =
			timeSystem.On
				.ThreadSleep(t =>
				{
					if (t.TotalMilliseconds.Equals(secondThreadMilliseconds))
					{
						isCalledFromSecondThread = true;
					}
				}).ExecuteWhileWaiting(() =>
				{
					// ReSharper disable once AccessToDisposedClosure
					try
					{
						listening.Set();
					}
					catch (ObjectDisposedException)
					{
						// Ignore any ObjectDisposedException
					}
				});
		new Thread(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				listening.Wait(1000, TestContext.Current.CancellationToken);
				timeSystem.Thread.Sleep(firstThreadMilliseconds);
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}).Start();
		wait.Wait();
		listening.Reset();

		new Thread(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				listening.Wait(1000, TestContext.Current.CancellationToken);
				// ReSharper disable once AccessToDisposedClosure
				if (!ms.IsSet)
				{
					// Should only trigger, if the second call to `Wait` still blocks
					timeSystem.Thread.Sleep(secondThreadMilliseconds);
				}
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}).Start();

		wait.Wait();
		ms.Set();
		await That(isCalledFromSecondThread).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Execute_ShouldBeExecutedBeforeWait(int milliseconds)
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

		await That(receivedMilliseconds).IsEqualTo(milliseconds);
		await That(isExecuted).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Execute_WithReturnValue_ShouldBeExecutedAndReturnValue(
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

		await That(receivedMilliseconds).IsEqualTo(milliseconds);
		await That(actualResult).IsEqualTo(result);
		await That(isExecuted).IsTrue();
	}

	[Fact]
	public async Task ExecuteWhileWaiting_ShouldExecuteCallback()
	{
		MockTimeSystem timeSystem = new();
		bool isExecuted = false;

		timeSystem.On
			.ThreadSleep()
			.ExecuteWhileWaiting(() =>
			{
				isExecuted = true;
				timeSystem.Thread.Sleep(10);
			})
			.Wait();

		await That(isExecuted).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task ExecuteWhileWaiting_WithReturnValue_ShouldExecuteCallback(int result)
	{
		MockTimeSystem timeSystem = new();
		bool isExecuted = false;

		int actualResult = timeSystem.On
			.ThreadSleep()
			.ExecuteWhileWaiting(() =>
			{
				isExecuted = true;
				timeSystem.Thread.Sleep(10);
				return result;
			})
			.Wait();

		await That(actualResult).IsEqualTo(result);
		await That(isExecuted).IsTrue();
	}
}
