using aweXpect.Chronology;
using aweXpect.Core;
using System.Collections.Generic;
using System.Diagnostics;
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
		InterceptingTimeProviderFactory factory = new();
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
			.Because("the advancing thread waits for each timer, so they fire in order while the clock is at their due time");
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 5.Seconds());
	}

	[Test]
	public async Task AdvanceBy_WhenTheCallerHoldsALockTheCallbackNeeds_ShouldNotDeadlock()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using SemaphoreSlim gate = new(1);
		TaskCompletionSource<bool> fired = new(TaskCreationOptions.RunContinuationsAsynchronously);
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			gate.Wait();
			gate.Release();
			fired.TrySetResult(true);
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		Task advance = Task.Run(() =>
		{
			gate.Wait();
			timeSystem.TimeProvider.AdvanceBy(5.Seconds());
			gate.Release();
		});
		Task advancedAndFired = Task.WhenAll(advance, fired.Task);
		bool isCompleted = await Task.WhenAny(advancedAndFired,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == advancedAndFired;

		await That(isCompleted).IsTrue()
			.Because("a callback that waits for the lock held by the advancing code must not block it");
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
	public async Task AutoAdvance_WhenACallbackBlocksBriefly_ShouldHoldTheClock()
	{
		MockTimeSystem timeSystem = new();
		List<string> events = [];
		using SemaphoreSlim gate = new(0);
		TaskCompletionSource<bool> secondFired = new(TaskCreationOptions.RunContinuationsAsynchronously);
		using ITimer first = timeSystem.Timer.New(_ =>
		{
			gate.Wait(5000);
			lock (events)
			{
				events.Add("first");
			}
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);
		using ITimer second = timeSystem.Timer.New(_ =>
		{
			lock (events)
			{
				events.Add("second");
			}

			secondFired.TrySetResult(true);
		}, null, 2.Seconds(), Timeout.InfiniteTimeSpan);

		Thread releaser = new(() =>
		{
			Thread.Sleep(100);
			gate.Release();
		});
		releaser.Start();
		bool isFired = await Task.WhenAny(secondFired.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == secondFired.Task;

		await That(isFired).IsTrue();
		await That(events).IsEqualTo(["first", "second",])
			.Because("the clock waits for a callback that briefly blocks on something else than the clock");
	}

	[Test]
	public async Task AutoAdvance_WhenACallbackWaitsForAnotherTimer_ShouldNotDeadlock()
	{
		MockTimeSystem timeSystem = new();
		TaskCompletionSource<bool> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			using ManualResetEventSlim otherFired = new();
			using ITimer other = timeSystem.Timer.New(_ => otherFired.Set(), null, 1.Seconds(),
				Timeout.InfiniteTimeSpan);
			otherFired.Wait();
			completed.TrySetResult(true);
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		bool isCompleted = await Task.WhenAny(completed.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == completed.Task;

		await That(isCompleted).IsTrue()
			.Because("a blocked callback must not keep the clock from reaching the timer it waits for");
	}

	[Test]
	public async Task AutoAdvance_WhenATimeChangedCallbackThrows_ShouldKeepFiringTheTimers()
	{
		MockTimeSystem timeSystem = new();
		int armed = 0;
		using ManualResetEventSlim thrown = new();
		using IAwaitableCallback<DateTime> _ = timeSystem.On.TimeChanged(_ =>
		{
			if (Interlocked.CompareExchange(ref armed, 2, 1) == 1)
			{
				thrown.Set();
				throw new InvalidOperationException("time changed");
			}
		});
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), 1.Seconds());
		ITimerMock timerMock = timeSystem.TimerHandler[0];
		Volatile.Write(ref armed, 1);
		thrown.Wait(TestContext.Current!.Execution.CancellationToken);
		long executionsAfterThrow = timerMock.ExecutionCount;

		Task continued = Task.Run(async () =>
		{
			while (timerMock.ExecutionCount <= executionsAfterThrow + 1)
			{
				await Task.Delay(10);
			}
		});
		bool isContinued = await Task.WhenAny(continued,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == continued;

		await That(isContinued).IsTrue()
			.Because("a throwing time change callback must not stop the clock for the auto-advancing timers");
	}

	[Test]
	public async Task AutoAdvance_WhenATimeoutCancelsADelay_ShouldStopTheClockAtTheTimeout()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new();
		using ITimer timer = timeSystem.Timer.New(s => ((CancellationTokenSource)s!).Cancel(), cts,
			1.Seconds(), Timeout.InfiniteTimeSpan);

		Task delay = timeSystem.Task.Delay(5.Seconds(), cts.Token);

		await That(delay.IsCanceled).IsTrue();
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 1.Seconds())
			.Because("the timeout canceled the delay after one second");
	}

	[Test]
	public async Task AutoAdvance_WhenATimeoutGetsAPeriod_ShouldMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		using CancellationTokenSource cts = new();
		TaskCompletionSource<bool> executedThreeTimes = new(TaskCreationOptions.RunContinuationsAsynchronously);
		int executions = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			if (Interlocked.Increment(ref executions) == 3)
			{
				executedThreeTimes.TrySetResult(true);
			}
		}, cts, 1.Seconds(), Timeout.InfiniteTimeSpan);

		timer.Change(1.Seconds(), 1.Seconds());
		bool isCompleted = await Task.WhenAny(executedThreeTimes.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == executedThreeTimes.Task;

		await That(isCompleted).IsTrue()
			.Because("whether a timer is a timeout depends on its current period");
	}

	[Test]
	public async Task AutoAdvance_WhenStateIsACancellationTokenSource_AndAPeriod_ShouldMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		using CancellationTokenSource shutdown = new();
		TaskCompletionSource<bool> executedThreeTimes = new(TaskCreationOptions.RunContinuationsAsynchronously);
		int executions = 0;

		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			if (Interlocked.Increment(ref executions) == 3)
			{
				executedThreeTimes.TrySetResult(true);
			}
		}, shutdown, 1.Seconds(), 1.Seconds());

		bool isCompleted = await Task.WhenAny(executedThreeTimes.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == executedThreeTimes.Task;

		await That(isCompleted).IsTrue()
			.Because("a periodic timer is never a timeout, even when its state is a CancellationTokenSource");
	}

	[Test]
	public async Task AutoAdvance_WhenStateIsACancellationTokenSource_ShouldFireWhenASleepPassesIt()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new();
		DateTime? canceledAt = null;
		using CancellationTokenRegistration _ = cts.Token.Register(() => canceledAt = timeSystem.DateTime.UtcNow);
		using ITimer timer = timeSystem.Timer.New(s => ((CancellationTokenSource)s!).Cancel(), cts,
			1.Seconds(), Timeout.InfiniteTimeSpan);

		timeSystem.Thread.Sleep(5.Seconds());

		await That(canceledAt).IsEqualTo(start + 1.Seconds());
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 5.Seconds());
	}

	[Test]
	public async Task AutoAdvance_WhenStateIsACancellationTokenSource_ShouldNotMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new();

		using ITimer timer = timeSystem.Timer.New(s => ((CancellationTokenSource)s!).Cancel(), cts,
			1.Seconds(), Timeout.InfiniteTimeSpan);

		await That(timeSystem.Scheduler.IsAdvancing).IsFalse()
			.Because("a one-shot timer that gets a CancellationTokenSource as state is a timeout");
		await That(cts.IsCancellationRequested).IsFalse();
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start);
	}

	[Test]
	public async Task Callback_ShouldRunOnAnotherThreadWithTheExecutionContextOfTheCreator()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		AsyncLocal<string> context = new() { Value = "creator", };
		string? observedContext = null;
		int? callbackThreadId = null;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			observedContext = context.Value;
			callbackThreadId = Environment.CurrentManagedThreadId;
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);
		context.Value = "advancer";

		timeSystem.TimeProvider.AdvanceBy(2.Seconds());

		await That(callbackThreadId).IsNotNull().And.IsNotEqualTo(Environment.CurrentManagedThreadId)
			.Because("like a real timer, the callback runs on another thread than the one that moves the clock");
		await That(observedContext).IsEqualTo("creator")
			.Because("like a real timer, the callback flows the execution context of its creator");
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
	public async Task DisableAutoAdvance_WhenACallbackWaitsForADelay_ShouldReleaseTheClockAtOnce()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		TaskCompletionSource<bool> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			timeSystem.Task.Delay(1.Seconds()).Wait();
			completed.TrySetResult(true);
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);
		Stopwatch stopwatch = Stopwatch.StartNew();

		timeSystem.TimeProvider.AdvanceBy(1.Seconds());
		TimeSpan released = stopwatch.Elapsed;
		timeSystem.TimeProvider.AdvanceBy(1.Seconds());
		bool isCompleted = await Task.WhenAny(completed.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == completed.Task;

		await That(released).IsLessThan(900.Milliseconds())
			.Because("a callback that waits for a delay of the time system releases the clock without the limit for blocked callbacks");
		await That(isCompleted).IsTrue();
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
	public async Task New_WhenAnotherThreadAdvancesPastTheDueTimeWhileScheduling_ShouldFire()
	{
		InterceptingTimeProviderFactory factory = new();
		MockTimeSystem timeSystem = new(factory, o => o.DisableAutoAdvance());
		TaskCompletionSource<bool> fired = new(TaskCreationOptions.RunContinuationsAsynchronously);
		factory.Provider!.OnNextRead = () => Task.Run(() => timeSystem.Scheduler.AdvanceBy(2.Seconds())).Wait();

		using ITimer timer = timeSystem.Timer.New(_ => fired.TrySetResult(true), null, 1.Seconds(),
			Timeout.InfiniteTimeSpan);
		bool isFired = await Task.WhenAny(fired.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == fired.Task;

		bool hasMovedTheClockWhileScheduling = factory.Provider.OnNextRead is null;

		await That(hasMovedTheClockWhileScheduling).IsTrue();
		await That(isFired).IsTrue()
			.Because("the clock passed the due time while the timer was scheduled, without another move to come");
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
	public async Task Sleep_WithTheProviderOfACustomFactory_ShouldFireTheTimerAtItsDueTime()
	{
		MockTimeSystem timeSystem = new(new InterceptingTimeProviderFactory());
		DateTime start = timeSystem.DateTime.UtcNow;
		DateTime? firedAt = null;
		using CancellationTokenSource cts = new();
		using ITimer timeout = timeSystem.Timer.New(_ => firedAt = timeSystem.DateTime.UtcNow, cts, 3.Seconds(),
			Timeout.InfiniteTimeSpan);

		timeSystem.Thread.Sleep(5.Seconds());

		await That(firedAt).IsEqualTo(start + 3.Seconds())
			.Because("the sleep moves a custom provider through the schedule as well");
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
	public async Task Wait_WhileItsCallbackRuns_ShouldHoldTheClock()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), 1.Seconds());
		using ITimer other = timeSystem.Timer.New(_ => { }, null, 100.Milliseconds(), 100.Milliseconds());
		ITimerMock timerMock = timeSystem.TimerHandler[0];
		DateTime? before = null;
		DateTime? after = null;
		Task wait = Task.Run(() => timerMock.Wait(timeout: 5000, callback: _ =>
		{
			before = timeSystem.DateTime.UtcNow;
			Thread.Sleep(100);
			after = timeSystem.DateTime.UtcNow;
		}));

		await AdvanceUntilCompleted(timeSystem, wait);

		await That(before).IsNotNull();
		await That(after).IsEqualTo(before)
			.Because("the timer waits in the handshake of the wait, which does not release the clock to the other timer");
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

	private sealed class InterceptingTimeProviderFactory : ITimeProviderFactory
	{
		public InterceptingTimeProvider? Provider { get; private set; }

		public ITimeProvider Create(Action<DateTime> onTimeChanged)
			=> Provider = new InterceptingTimeProvider(TimeProviderFactory
				.Use(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))
				.Create(onTimeChanged));
	}

	/// <summary>
	///     A provider that is not the built-in one, which runs <see cref="OnNextRead" /> once on the next
	///     <see cref="Read" />, which scheduling a timer calls after it read the clock and before it adds the timer to the
	///     schedule.
	/// </summary>
	private sealed class InterceptingTimeProvider(ITimeProvider inner) : ITimeProvider
	{
		public Action? OnNextRead { get; set; }

		public long ElapsedTicks => inner.ElapsedTicks;

		public TimeZoneInfo LocalTimeZone
		{
			get => inner.LocalTimeZone;
			set => inner.LocalTimeZone = value;
		}

		public DateTime MaxValue
		{
			get => inner.MaxValue;
			set => inner.MaxValue = value;
		}

		public DateTime MinValue
		{
			get => inner.MinValue;
			set => inner.MinValue = value;
		}

		public DateTime StartTime => inner.StartTime;

		public DateTime UnixEpoch
		{
			get => inner.UnixEpoch;
			set => inner.UnixEpoch = value;
		}

		public void AdvanceBy(TimeSpan interval) => inner.AdvanceBy(interval);

		public TimeZoneInfo FindSystemTimeZoneById(string id) => inner.FindSystemTimeZoneById(id);

		public System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
			=> inner.GetSystemTimeZones();

		public DateTime Read()
		{
			Action? onRead = OnNextRead;
			OnNextRead = null;
			onRead?.Invoke();
			return inner.Read();
		}

		public void RegisterTimeZone(TimeZoneInfo timeZone) => inner.RegisterTimeZone(timeZone);

		public void SetTo(DateTime value) => inner.SetTo(value);
	}
}
