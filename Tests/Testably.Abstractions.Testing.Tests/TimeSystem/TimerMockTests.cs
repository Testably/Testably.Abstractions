using aweXpect.Chronology;
using aweXpect.Core;
using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.Testing.TimeSystem;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

// ReSharper disable UseAwaitUsing
public class TimerMockTests
{
	[Test]
	public async Task AdvanceBy_OnTheProviderOfACustomFactory_ShouldFireTheDueTimers()
	{
		ExposingTimeProviderFactory factory = new();
		MockTimeSystem timeSystem = new(factory, o => o.DisableAutoAdvance());
		DateTime start = timeSystem.DateTime.UtcNow;
		TaskCompletionSource<DateTime> fired = new(TaskCreationOptions.RunContinuationsAsynchronously);
		using ITimer timer = timeSystem.Timer.New(_ => fired.TrySetResult(timeSystem.DateTime.UtcNow), null,
			1.Seconds(), Timeout.InfiniteTimeSpan);

		factory.Provider!.AdvanceBy(2.Seconds());
		bool isFired = await Task.WhenAny(fired.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == fired.Task;

		await That(isFired).IsTrue();
		await That(await fired.Task).IsEqualTo(start + 2.Seconds())
			.Because("the clock was already past the due time when the provider was advanced directly");
	}

	[Test]
	public async Task AdvanceBy_ShouldFireTimersInTheOrderOfTheirDueTime()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		DateTime start = timeSystem.DateTime.UtcNow;
		List<TimeSpan> callbackTimes = [];
		using ITimer timer3 = timeSystem.Timer.New(_ => callbackTimes.Add(timeSystem.DateTime.UtcNow - start),
			null, 3.Seconds(), Timeout.InfiniteTimeSpan);
		using ITimer timer1 = timeSystem.Timer.New(_ => callbackTimes.Add(timeSystem.DateTime.UtcNow - start),
			null, 1.Seconds(), Timeout.InfiniteTimeSpan);
		using ITimer timer2 = timeSystem.Timer.New(_ => callbackTimes.Add(timeSystem.DateTime.UtcNow - start),
			null, 2.Seconds(), 2.Seconds());

		timeSystem.TimeProvider.AdvanceBy(5.Seconds());

		await That(callbackTimes).IsEqualTo([1.Seconds(), 2.Seconds(), 3.Seconds(), 4.Seconds(),])
			.Because("the timers fire on the advancing thread, in order, while the clock is at their due time");
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 5.Seconds());
	}

	[Test]
	public async Task AdvanceBy_WhenATimeChangedCallbackRuns_ShouldNotBlockDisposingATimerOnAnotherThread()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		ITimer other = timeSystem.Timer.New(_ => { }, null, 10.Seconds(), Timeout.InfiniteTimeSpan);
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), Timeout.InfiniteTimeSpan);
		bool? disposedInTime = null;
		using IAwaitableCallback<DateTime> _ = timeSystem.On.TimeChanged(_ =>
			disposedInTime ??= Task.Run(() => other.Dispose()).Wait(2000));

		timeSystem.TimeProvider.AdvanceBy(2.Seconds());

		await That(disposedInTime).IsTrue()
			.Because("the schedule must not stay locked while the clock notifies about a time change");
	}

	[Test]
	public async Task AdvanceBy_WhenATimeChangedCallbackThrows_ShouldKeepTheTimerScheduled()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		int executions = 0;
		using ITimer timer = timeSystem.Timer.New(_ => Interlocked.Increment(ref executions), null,
			1.Seconds(), Timeout.InfiniteTimeSpan);
		bool hasThrown = false;
		using IAwaitableCallback<DateTime> _ = timeSystem.On.TimeChanged(_ =>
		{
			if (!hasThrown)
			{
				hasThrown = true;
				throw new InvalidOperationException("time changed");
			}
		});

		void Act() => timeSystem.TimeProvider.AdvanceBy(2.Seconds());

		await That(Act).ThrowsExactly<InvalidOperationException>().WithMessage("time changed");
		timeSystem.TimeProvider.AdvanceBy(1.Seconds());
		await That(executions).IsEqualTo(1)
			.Because("the timer stays scheduled when moving the clock to its due time throws");
	}

	[Test]
	public async Task AutoAdvance_ShouldFireTimersInTheOrderOfTheirDueTime()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		List<TimeSpan> callbackTimes = [];
		List<ITimer> timers = [];
		TaskCompletionSource<bool> allFired = new(TaskCreationOptions.RunContinuationsAsynchronously);

		void AddCallbackTime(object? _)
		{
			lock (callbackTimes)
			{
				callbackTimes.Add(timeSystem.DateTime.UtcNow - start);
				if (callbackTimes.Count == 3)
				{
					allFired.TrySetResult(true);
				}
			}
		}

		using ITimer starter = timeSystem.Timer.New(_ =>
		{
			timers.Add(timeSystem.Timer.New(AddCallbackTime, null, 3.Seconds(), Timeout.InfiniteTimeSpan));
			timers.Add(timeSystem.Timer.New(AddCallbackTime, null, 1.Seconds(), Timeout.InfiniteTimeSpan));
			timers.Add(timeSystem.Timer.New(AddCallbackTime, null, 2.Seconds(), Timeout.InfiniteTimeSpan));
		}, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
		bool isCompleted = await Task.WhenAny(allFired.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == allFired.Task;
		timers.ForEach(timer => timer.Dispose());

		await That(isCompleted).IsTrue();
		await That(callbackTimes).IsEqualTo([1.Seconds(), 2.Seconds(), 3.Seconds(),])
			.Because("all auto-advancing timers move the clock on one thread, in the order of their due time");
	}

	[Test]
	public async Task AutoAdvance_WhenStateIsACancellationTokenSource_ShouldStillMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		using CancellationTokenSource cts = new();
		TaskCompletionSource<bool> canceled = new(TaskCreationOptions.RunContinuationsAsynchronously);
		using CancellationTokenRegistration _ = cts.Token.Register(() => canceled.TrySetResult(true));

		using ITimer timer = timeSystem.Timer.New(s => ((CancellationTokenSource)s!).Cancel(), cts,
			1.Seconds(), Timeout.InfiniteTimeSpan);

		bool isCanceled = await Task.WhenAny(canceled.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == canceled.Task;

		await That(isCanceled).IsTrue()
			.Because("only the timeouts that the BCL creates on the TimeProvider bridge do not move the clock");
	}

	[Test]
	public async Task Change_Concurrently_ShouldNotKeepAnyScheduleAfterDispose()
	{
		for (int i = 0; i < 20; i++)
		{
			MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
			int executions = 0;
			ITimer timer = timeSystem.Timer.New(_ => Interlocked.Increment(ref executions), null,
				Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

			Parallel.For(0, 50, _ => timer.Change(1.Seconds(), Timeout.InfiniteTimeSpan));
			timer.Dispose();
			timeSystem.TimeProvider.AdvanceBy(2.Seconds());

			await That(executions).IsEqualTo(0)
				.Because($"a disposed timer must not fire, even after concurrent changes (iteration {i})");
		}
	}

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
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using ITimerMock timer = (ITimerMock)timeSystem.Timer.New(
			_ => { }, null, 1.Seconds(), 2.Seconds());

		await That(timer).Executed().Never().Within(50.Milliseconds());

		// Advance past dueTime (1s): should trigger first callback
		timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		await That(timer).Executed().AtLeast(1.Times()).Within(10.Seconds());
		await That(timer.ExecutionCount).IsEqualTo(1L);

		// Advance past one period (2s): should trigger second callback
		timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		await That(timer).Executed().AtLeast(2.Times()).Within(10.Seconds());
		await That(timer.ExecutionCount).IsEqualTo(2L);

		// Advance past two periods (4s): should trigger two more callbacks
		timeSystem.TimeProvider.AdvanceBy(4.Seconds());
		await That(timer).Executed().AtLeast(4.Times()).Within(10.Seconds());
		await That(timer.ExecutionCount).IsEqualTo(4L);
	}

	[Test]
	public async Task DisableAutoAdvance_ShouldNotExecuteTimerBeforeTimeElapsed()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using ITimerMock timer = (ITimerMock)timeSystem.Timer.New(
			_ => { }, null, 1.Seconds(), 2.Seconds());

		await That(timer).Executed().Never().Within(50.Milliseconds());
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
		DateTime start = timeSystem.DateTime.UtcNow;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			callbackTime = timeSystem.DateTime.UtcNow;
			// ReSharper disable once AccessToDisposedClosure
			callbackExecuted.Release();
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		await Task.Delay(50.Milliseconds(), token);
		timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		await callbackExecuted.WaitAsync(token);

		await That(callbackTime).IsEqualTo(start + 1.Seconds())
			.Because("the callback runs while the clock is at the due time of the timer");
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
			.WithMessage("Cannot access a disposed object.");
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
	public async Task ExecutionCount_AfterDispose_ShouldStopIncreasing()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);
		ITimerMock mock = timerHandler[0];

		mock.Wait(5, callback: t => t.Dispose(), timeout: 10000);
		long countAfterDispose = mock.ExecutionCount;
		await Task.Delay(200, TestContext.Current!.Execution.CancellationToken);

		await That(mock.ExecutionCount).IsEqualTo(countAfterDispose);
	}

	[Test]
	public async Task ExecutionCount_PersistsAcrossWaitCalls()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);

		await That(timerHandler[0].ExecutionCount).IsEqualTo(0L);

		timerHandler[0].Wait(3, timeout: 10000);
		await That(timerHandler[0].ExecutionCount).IsGreaterThanOrEqualTo(3L);

		long afterFirstWait = timerHandler[0].ExecutionCount;
		timerHandler[0].Wait(3, timeout: 10000);
		await That(timerHandler[0].ExecutionCount).IsGreaterThanOrEqualTo(afterFirstWait + 3L);
	}

	[Test]
	public async Task ExecutionCount_ShouldStartAtZero()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);

		await That(timerHandler[0].ExecutionCount).IsEqualTo(0L);
	}

	[Test]
	public async Task New_WithMaxValueDueTimeAndPeriod_ShouldNotFire()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		int executions = 0;
		using ITimer timer = timeSystem.Timer.New(_ => Interlocked.Increment(ref executions), null,
			TimeSpan.MaxValue, TimeSpan.MaxValue);

		timeSystem.TimeProvider.AdvanceBy(1.Seconds());

		await That(executions).IsEqualTo(0)
			.Because("a due time beyond the end of the clock must not overflow into the past");
	}

	[Test]
	public async Task New_WithMaxValueDueTimeAndPeriod_WithAutoAdvance_ShouldNotMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, TimeSpan.MaxValue, TimeSpan.MaxValue);

		await That(timeSystem.Scheduler.IsAdvancing).IsFalse()
			.Because("a due time beyond the end of the clock is never reached");
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start);
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
	public async Task Wait_AfterATimeout_ShouldNotBlockTheThreadThatAdvancesTheClock()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), 1.Seconds());
		ITimerMock timerMock = timeSystem.TimerHandler[0];

		void Wait() => timerMock.Wait(executionCount: 2, timeout: 50);

		await That(Wait).ThrowsExactly<TimeoutException>();
		Task advance = Task.Run(() => timeSystem.TimeProvider.AdvanceBy(5.Seconds()));
		bool isCompleted = await Task.WhenAny(advance,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == advance;

		await That(isCompleted).IsTrue()
			.Because("nobody waits for the timer anymore after the wait timed out");
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
	public async Task Wait_WhenTheCallbackDisposesTheTimer_ShouldNotThrow()
	{
		MockTimeSystem timeSystem = new MockTimeSystem(o => o.DisableAutoAdvance())
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimer? timer = null;
		timer = timeSystem.Timer.New(_ => timer!.Dispose(), null, 1.Seconds(), 1.Seconds());
		ITimerMock timerMock = timeSystem.TimerHandler[0];
		Task wait = Task.Run(() => timerMock.Wait(timeout: 5000));

		await AdvanceUntilCompleted(timeSystem, wait);

		await That(timerMock.ExecutionCount).IsEqualTo(1)
			.Because("the callback disposed the timer at its first execution");
	}

	[Test]
	public async Task Wait_WhenTheTimerExecutesAgainAfterwards_ShouldNotThrow()
	{
		MockTimeSystem timeSystem = new MockTimeSystem(o => o.DisableAutoAdvance())
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), 1.Seconds());
		ITimerMock timerMock = timeSystem.TimerHandler[0];
		Task wait = Task.Run(() => timerMock.Wait(timeout: 5000));
		await AdvanceUntilCompleted(timeSystem, wait);
		long executionCount = timerMock.ExecutionCount;

		void Act() => timeSystem.TimeProvider.AdvanceBy(2.Seconds());

		await That(Act).DoesNotThrow();
		await That(timerMock.ExecutionCount).IsEqualTo(executionCount + 2);
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

	/// <summary>
	///     Advances the clock step by step until the <paramref name="task" /> completes, as a <c>Wait</c> on another
	///     thread starts the timer only after an unknown time.
	/// </summary>
	private static async Task AdvanceUntilCompleted(MockTimeSystem timeSystem, Task task)
	{
		while (!task.IsCompleted)
		{
			timeSystem.TimeProvider.AdvanceBy(1.Seconds());
			await Task.Delay(1, TestContext.Current!.Execution.CancellationToken);
		}

		await task;
	}

	private sealed class DummyWaitHandle : WaitHandle;

	private sealed class ExposingTimeProviderFactory : ITimeProviderFactory
	{
		public ITimeProvider? Provider { get; private set; }

		public ITimeProvider Create(Action<DateTime> onTimeChanged)
			=> Provider = TimeProviderFactory.Use(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))
				.Create(onTimeChanged);
	}
}
