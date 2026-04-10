using aweXpect.Chronology;
using System.Threading;
using Testably.Abstractions.Testing.TimeSystem;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

// ReSharper disable UseAwaitUsing
public class TimerMockTests
{
	[Test]
	[Arguments(-1)]
	[Arguments(0)]
	[Arguments(2000)]
	public async Task Change_ValidDueTimeValue_ShouldNotThrowException(int dueTime)
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(dueTime, 0);
		});

		await That(exception).IsNull();
	}

	[Test]
	[Arguments(-1)]
	[Arguments(0)]
	[Arguments(2000)]
	public async Task Change_ValidPeriodValue_ShouldNotThrowException(int period)
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, period);
		});

		await That(exception).IsNull();
	}

	[Test]
	public async Task DisableAutoAdvance_ShouldExecuteTimerLimitedNumberOfTimes()
	{
		int callbackCount = 0;
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(30.Seconds());
		CancellationToken token = cts.Token;
		using SemaphoreSlim callbackExecuted = new(0);
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToModifiedClosure
			Interlocked.Increment(ref callbackCount);
			// ReSharper disable once AccessToDisposedClosure
			callbackExecuted.Release();
		}, null, 1.Seconds(), 2.Seconds());

		await Task.Delay(50.Milliseconds(), token);
		await That(Volatile.Read(ref callbackCount)).IsEqualTo(0);

		// Advance past dueTime (1s): should trigger first callback
		timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		await callbackExecuted.WaitAsync(token);
		await That(Volatile.Read(ref callbackCount)).IsEqualTo(1);

		// Advance past one period (2s): should trigger second callback
		await Task.Delay(50.Milliseconds(), token);
		timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		await callbackExecuted.WaitAsync(token);
		await That(Volatile.Read(ref callbackCount)).IsEqualTo(2);

		// Advance past two periods (4s): should trigger two more callbacks
		await Task.Delay(50.Milliseconds(), token);
		timeSystem.TimeProvider.AdvanceBy(4.Seconds());
		await callbackExecuted.WaitAsync(token);
		await Task.Delay(50.Milliseconds(), token);
		timeSystem.TimeProvider.AdvanceBy(0.Seconds());
		await callbackExecuted.WaitAsync(token);
		await That(Volatile.Read(ref callbackCount)).IsEqualTo(4);
	}

	[Test]
	public async Task DisableAutoAdvance_ShouldNotExecuteTimerBeforeTimeElapsed()
	{
		int callbackCount = 0;
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(30.Seconds());
		CancellationToken token = cts.Token;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToModifiedClosure
			Interlocked.Increment(ref callbackCount);
		}, null, 1.Seconds(), 2.Seconds());

		await Task.Delay(50.Milliseconds(), token);
		await That(Volatile.Read(ref callbackCount)).IsEqualTo(0);
	}

	[Test]
	public async Task DisableAutoAdvance_ShouldStartTimerWhenTimeElapsed()
	{
		DateTime callbackTime = DateTime.MinValue;
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(30.Seconds());
		CancellationToken token = cts.Token;
		using SemaphoreSlim callbackExecuted = new(0);
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			callbackTime = timeSystem.DateTime.UtcNow;
			// ReSharper disable once AccessToDisposedClosure
			callbackExecuted.Release();
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		await Task.Delay(50.Milliseconds(), token);
		timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		await callbackExecuted.WaitAsync(token);
		DateTime after = timeSystem.DateTime.UtcNow;

		await That(callbackTime).IsEqualTo(after);
	}

	[Test]
	public async Task Dispose_ShouldDisposeTimer()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		// ReSharper disable once DisposeOnUsingVariable
		timer.Dispose();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		}

#if NET8_0_OR_GREATER
		await That(Act).DoesNotThrow();
#else
		await That(Act).Throws<ObjectDisposedException>()
			.Whose(x => x.Message,
				it => it.Satisfies(m
					=> m!.Contains("Cannot access a disposed object.", StringComparison.Ordinal) &&
					   m!.Contains(nameof(ITimer.Change), StringComparison.Ordinal)));
#endif
	}

	[Test]
	public async Task Dispose_WithUnknownWaitHandle_ShouldThrowNotSupportedException()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using DummyWaitHandle waitHandle = new();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer
				// ReSharper disable once AccessToDisposedClosure
				.Dispose(waitHandle);
		}

		await That(Act).ThrowsExactly<NotSupportedException>()
			.WithMessage($"*{typeof(DummyWaitHandle)}*").AsWildcard();
	}

#if FEATURE_ASYNC_DISPOSABLE
	[Test]
	public async Task DisposeAsync_ShouldDisposeTimer()
	{
		MockTimeSystem timeSystem = new();
		await using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		// ReSharper disable once DisposeOnUsingVariable
		await timer.DisposeAsync();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		}

#if NET8_0_OR_GREATER
		await That(Act).DoesNotThrow();
#else
		await That(Act).Throws<ObjectDisposedException>();
#endif
	}
#endif

	[Test]
	public async Task Exception_WhenSwallowExceptionsIsNotSet_ShouldStopTimer()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(
				TimerMode.StartOnMockWait,
				swallowExceptions: false));
		Exception expectedException = new("foo");
		int count = 0;
		using ITimer timer = timeSystem.Timer.New(
			_ =>
			{
				if (++count == 1)
				{
					throw expectedException;
				}
			}, null, 0, 20);

		Exception? exception = Record.Exception(() =>
		{
			timeSystem.TimerHandler[0].Wait();
		});

		await Task.Delay(10, TestContext.Current!.Execution.CancellationToken);
		await That(exception).IsEqualTo(expectedException);
		await That(count).IsEqualTo(1);
	}

	[Test]
	public async Task Exception_WhenSwallowExceptionsIsNotSet_ShouldThrowExceptionOnWait()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(
				TimerMode.StartOnMockWait,
				swallowExceptions: false));
		Exception expectedException = new("foo");
		using ITimer timer = timeSystem.Timer.New(
			_ => throw expectedException, null, 0, 20);

		Exception? exception = Record.Exception(() =>
		{
			timeSystem.TimerHandler[0].Wait();
		});

		await That(exception).IsEqualTo(expectedException);
	}

	[Test]
	public async Task Exception_WhenSwallowExceptionsIsSet_ShouldContinueTimerExecution()
	{
		MockTimeSystem timeSystem = new();
		timeSystem.WithTimerStrategy(
			new TimerStrategy(swallowExceptions: true));
		Exception exception = new("foo");
		int count = 0;
		using ManualResetEventSlim ms = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				if (count++ == 1)
				{
					throw exception;
				}

				if (count == 3)
				{
					ms.Set();
				}
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}, null, 0, 20);

		await That(ms.Wait(10000, TestContext.Current!.Execution.CancellationToken)).IsTrue();

		await That(count).IsGreaterThanOrEqualTo(3);
	}

	[Test]
	public async Task New_WithStartOnMockWaitMode_ShouldOnlyStartWhenCallingWait()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ => count++, null, 0, 100);

		await Task.Delay(10, TestContext.Current!.Execution.CancellationToken);
		await That(count).IsEqualTo(0);
		timerHandler[0].Wait();
		await That(count).IsGreaterThan(0);
	}

	[Test]
	public async Task ShouldNotBeAffectedByTimeChange()
	{
		int timerCount = 0;
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(30.Seconds());
		CancellationToken token = cts.Token;
		using SemaphoreSlim ticks = new(0);
		using SemaphoreSlim advanced = new(0);
		Task timerTask = Task.Run(async () =>
		{
			try
			{
				using ITimer timer = timeSystem.Timer.New(_ =>
				{
					// ReSharper disable once AccessToModifiedClosure
					Interlocked.Increment(ref timerCount);
					// ReSharper disable AccessToDisposedClosure
					ticks.Release();
					advanced.Wait(token);
					// ReSharper restore AccessToDisposedClosure
				}, null, TimeSpan.Zero, 1.Seconds());

				await Task.Delay(Timeout.InfiniteTimeSpan, token);
			}
			catch (OperationCanceledException)
			{
				// Ignore cancellation
			}
		}, token);

		await ticks.WaitAsync(token);

		timeSystem.TimeProvider.AdvanceBy(1.Seconds());
		advanced.Release();
		await ticks.WaitAsync(token);

		timeSystem.TimeProvider.AdvanceBy(1.Seconds());
		advanced.Release();
		await ticks.WaitAsync(token);

		await That(Volatile.Read(ref timerCount)).IsEqualTo(3);

		// Changing the wall clock time should not affect the timer
		timeSystem.TimeProvider.SetTo(timeSystem.DateTime.Now + 10.Seconds());

		for (int i = 0; i < 10; i++)
		{
			timeSystem.TimeProvider.AdvanceBy(1.Seconds());
			advanced.Release();
			await ticks.WaitAsync(token);
		}

		await That(Volatile.Read(ref timerCount)).IsEqualTo(13);

		timeSystem.TimeProvider.AdvanceBy(1.Seconds());
		advanced.Release();
		await ticks.WaitAsync(token);

		await That(Volatile.Read(ref timerCount)).IsEqualTo(14);
		cts.Cancel();
		await timerTask;
	}

	[Test]
	public async Task Wait_Infinite_ShouldBeValidTimeout()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 0, 100);

		Exception? exception = Record.Exception(() =>
		{
			timerHandler[0].Wait(timeout: Timeout.Infinite);
		});

		await That(exception).IsNull();
	}

	[Test]
	public async Task Wait_InvalidExecutionCount_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);

		void Act()
		{
			timerHandler[0].Wait(0);
		}

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("executionCount")
			.And
			.WithMessage("Execution count must be greater than zero.").AsPrefix();
	}

	[Test]
	public async Task Wait_InvalidTimeout_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);

		void Act()
		{
			timerHandler[0].Wait(timeout: -2);
		}

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("timeout");
	}

	[Test]
	public async Task Wait_TimeoutExpired_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		using ManualResetEventSlim ms = new();

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				count++;
				ms.Wait(TestContext.Current!.Execution.CancellationToken);
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}, null, 0, 100);

		Exception? exception = Record.Exception(() =>
		{
			timerHandler[0].Wait(10, 300);
		});
		ms.Set();

		await That(exception).IsExactly<TimeoutException>();
		await That(count).IsGreaterThan(0);
	}

	[Test]
	public async Task Wait_Twice_ShouldContinueExecutionsAfterFirstWait()
	{
		int executionCount = 10;
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
			Console.WriteLine($"Execute: {count}");
		}, null, 0, 100);

		Console.WriteLine($"Waiting {executionCount} times...");
		timerHandler[0].Wait(executionCount, callback: _ =>
			Console.WriteLine("Waiting completed."));
		Console.WriteLine($"Waiting {executionCount} times...");
		timerHandler[0].Wait(executionCount, callback: t =>
		{
			Console.WriteLine("Waiting completed.");
			Console.WriteLine("Disposing...");
			t.Dispose();
			Console.WriteLine("Disposed.");
		}, timeout: 10000);
		await That(count).IsGreaterThanOrEqualTo(2 * executionCount);
	}

	[Test]
	public async Task Wait_WithExecutionCount_ShouldWaitForSpecifiedNumberOfExecutions()
	{
		int executionCount = 10;
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
			Console.WriteLine($"Execute: {count}");
		}, null, 0, 100);

		await That(count).IsEqualTo(0);
		timerHandler[0].Wait(executionCount, callback: t =>
		{
			Console.WriteLine("Disposing...");
			t.Dispose();
			Console.WriteLine("Disposed.");
		}, timeout: 10000);
		Console.WriteLine("Waiting 100ms...");
		await Task.Delay(1000, TestContext.Current!.Execution.CancellationToken);
		Console.WriteLine("Waiting completed.");
		await That(count).IsEqualTo(executionCount);
	}

	private sealed class DummyWaitHandle : WaitHandle;
}
