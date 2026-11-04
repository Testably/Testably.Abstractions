using aweXpect.Chronology;
using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerHandlerTests
{
	[Test]
	public async Task AdvanceToNextTimer_ShouldFireATimeout()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new();
		using ITimer timeout = timeSystem.Timer.New(s => ((CancellationTokenSource)s!).Cancel(), cts,
			5.Seconds(), Timeout.InfiniteTimeSpan);

		bool result = timeSystem.TimerHandler.AdvanceToNextTimer();

		await That(result).IsTrue();
		await That(cts.IsCancellationRequested).IsTrue();
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 5.Seconds())
			.Because("stepping fires the next timer, also a timeout that does not move the clock on its own");
	}

	[Test]
	public async Task AdvanceToNextTimer_ShouldStepThroughTheTimersInTheOrderOfTheirDueTime()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		DateTime start = timeSystem.DateTime.UtcNow;
		List<string> fired = [];
		List<TimeSpan> times = [];
		using ITimer timer3 = timeSystem.Timer.New(_ => fired.Add("3s"), null, 3.Seconds(),
			Timeout.InfiniteTimeSpan);
		using ITimer timer1 = timeSystem.Timer.New(_ => fired.Add("1s"), null, 1.Seconds(),
			Timeout.InfiniteTimeSpan);
		using ITimer timer2 = timeSystem.Timer.New(_ => fired.Add("2s"), null, 2.Seconds(), 2.Seconds());

		for (int i = 0; i < 4; i++)
		{
			timeSystem.TimerHandler.AdvanceToNextTimer();
			times.Add(timeSystem.DateTime.UtcNow - start);
		}

		await That(times).IsEqualTo([1.Seconds(), 2.Seconds(), 3.Seconds(), 4.Seconds(),]);
		await That(fired).IsEqualTo(["1s", "2s", "3s", "2s",]);
	}

	[Test]
	public async Task AdvanceToNextTimer_WhenTheCallbackDoesNotComplete_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using ManualResetEventSlim gate = new();
		using ITimer timer = timeSystem.Timer.New(_ => gate.Wait(), null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		void Act() => timeSystem.TimerHandler.AdvanceToNextTimer(100);

		try
		{
			await That(Act).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the next timer did not complete in 100ms.");
		}
		finally
		{
			gate.Set();
		}
	}

	[Test]
	public async Task AdvanceToNextTimer_WithInvalidTimeout_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();

		void Act() => timeSystem.TimerHandler.AdvanceToNextTimer(-2);

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("timeout");
	}

	[Test]
	public async Task AdvanceToNextTimer_WithoutTimers_ShouldReturnFalseAndNotMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;

		bool result = timeSystem.TimerHandler.AdvanceToNextTimer();

		await That(result).IsFalse();
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start);
	}

	[Test]
	public async Task Index_AccessDisposedIndex_ShouldThrowException()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using (timeSystem.Timer.New(_ => { }, null, 0, 100))
		{
			// Immediately dispose the created timer.
		}

		Exception? exception = Record.Exception(() =>
		{
			_ = sut[0];
		});

		await That(exception).IsExactly<KeyNotFoundException>();
	}

	[Test]
	public async Task Index_MultipleTimers_ShouldIncrement()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using ITimer timer0 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		using ITimer timer1 = timeSystem.Timer.New(_ => { }, null, 0, 100);

		await That(sut[0]).IsEqualTo(timer0);
		await That(sut[1]).IsEqualTo(timer1);
	}

	[Test]
	public async Task Index_ShouldNotReuseDisposedIndexes()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using ITimer timer0 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		using ITimer timer1 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		// ReSharper disable once DisposeOnUsingVariable
		timer0.Dispose();
		using ITimer timer2 = timeSystem.Timer.New(_ => { }, null, 0, 100);

		await That(sut[1]).IsEqualTo(timer1);
		await That(sut[2]).IsEqualTo(timer2);
	}

	[Test]
	public async Task WaitUntilIdle_ShouldWaitForTheAutoAdvancingTimers()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		List<TimeSpan> times = [];
		List<ITimer> timers = [];
		void AddTime(object? _) => times.Add(timeSystem.DateTime.UtcNow - start);

		// The starter holds the clock while it creates the timers, so that the clock cannot pass one of them before
		// all are scheduled.
		using ITimer starter = timeSystem.Timer.New(_ =>
		{
			timers.Add(timeSystem.Timer.New(AddTime, null, 3.Seconds(), Timeout.InfiniteTimeSpan));
			timers.Add(timeSystem.Timer.New(AddTime, null, 1.Seconds(), Timeout.InfiniteTimeSpan));
			timers.Add(timeSystem.Timer.New(AddTime, null, 2.Seconds(), Timeout.InfiniteTimeSpan));
		}, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

		timeSystem.TimerHandler.WaitUntilIdle();
		timers.ForEach(timer => timer.Dispose());

		await That(times).IsEqualTo([1.Seconds(), 2.Seconds(), 3.Seconds(),]);
	}

	[Test]
	public async Task WaitUntilIdle_WhenACallbackRunsLongerThanTheClockWaits_ShouldWaitForIt()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		int isCompleted = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			Thread.Sleep(1500);
			Volatile.Write(ref isCompleted, 1);
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);
		timeSystem.TimeProvider.AdvanceBy(1.Seconds());

		timeSystem.TimerHandler.WaitUntilIdle();

		await That(Volatile.Read(ref isCompleted)).IsEqualTo(1)
			.Because("the callback still runs after the clock stopped waiting for it");
	}

	[Test]
	public async Task WaitUntilIdle_WhenACallbackWaitsForTheClock_ShouldReturn()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using ManualResetEventSlim delayed = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			timeSystem.Task.Delay(5.Seconds()).Wait();
			delayed.Set();
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);
		timeSystem.TimeProvider.AdvanceBy(1.Seconds());

		timeSystem.TimerHandler.WaitUntilIdle();

		await That(delayed.IsSet).IsFalse();
		timeSystem.TimeProvider.AdvanceBy(5.Seconds());
		await That(delayed.Wait(10000, TestContext.Current!.Execution.CancellationToken)).IsTrue();
	}

	[Test]
	public async Task WaitUntilIdle_WhenOnlyATimeoutIsLeft_ShouldReturn()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new();
		using ITimer timeout = timeSystem.Timer.New(s => ((CancellationTokenSource)s!).Cancel(), cts,
			5.Seconds(), Timeout.InfiniteTimeSpan);
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		timeSystem.TimerHandler.WaitUntilIdle();

		await That(cts.IsCancellationRequested).IsFalse();
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 1.Seconds())
			.Because("a timeout does not move the clock on its own");
	}

	[Test]
	public async Task WaitUntilIdle_WithAPeriodicTimerAndAutoAdvance_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), 1.Seconds());

		void Act() => timeSystem.TimerHandler.WaitUntilIdle(100);

		await That(Act).ThrowsExactly<TimeoutException>()
			.WithMessage(
				"The timers did not become idle in 100ms: * running callback(s), * due timer(s) and * timer(s) that advance the clock.")
			.AsWildcard();
	}

	[Test]
	public async Task WaitUntilIdle_WithInvalidTimeout_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();

		void Act() => timeSystem.TimerHandler.WaitUntilIdle(-2);

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("timeout");
	}

#if FEATURE_TIMEPROVIDER
	[Test]
	public async Task WaitUntilIdleAsync_WhenADelayOnTheTimeProviderWaitsForATimeout_ShouldReturn()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider timeProvider = timeSystem.ToTimeProvider();
		using CancellationTokenSource cts = new(5.Seconds(), timeProvider);
		Task delay = Task.Delay(Timeout.InfiniteTimeSpan, timeProvider, cts.Token);

		await timeSystem.TimerHandler.WaitUntilIdleAsync(
			cancellationToken: TestContext.Current!.Execution.CancellationToken);

		await That(delay.IsCompleted).IsFalse()
			.Because("only the timeout is left, and a timeout does not move the clock");
		timeSystem.TimeProvider.AdvanceBy(5.Seconds());
		await That(delay.IsCanceled).IsTrue();
	}
#endif

	[Test]
	public async Task WaitUntilIdleAsync_ShouldWaitForTheAutoAdvancingTimers()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		List<TimeSpan> times = [];
		List<ITimer> timers = [];
		void AddTime(object? _) => times.Add(timeSystem.DateTime.UtcNow - start);

		// The starter holds the clock while it creates the timers, so that the clock cannot pass one of them before
		// all are scheduled.
		using ITimer starter = timeSystem.Timer.New(_ =>
		{
			timers.Add(timeSystem.Timer.New(AddTime, null, 2.Seconds(), Timeout.InfiniteTimeSpan));
			timers.Add(timeSystem.Timer.New(AddTime, null, 1.Seconds(), Timeout.InfiniteTimeSpan));
		}, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

		await timeSystem.TimerHandler.WaitUntilIdleAsync(
			cancellationToken: TestContext.Current!.Execution.CancellationToken);
		timers.ForEach(timer => timer.Dispose());

		await That(times).IsEqualTo([1.Seconds(), 2.Seconds(),]);
	}

	[Test]
	public async Task WaitUntilIdleAsync_WhenTheLastAdvancingTimerIsDisposed_ShouldComplete()
	{
		MockTimeSystem timeSystem = new();
		// Holds the advancing loop back, so that the timer stays scheduled until it is disposed.
		timeSystem.Scheduler.BeginCreatingTimer();
		try
		{
			using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), Timeout.InfiniteTimeSpan);
			Task wait = timeSystem.TimerHandler.WaitUntilIdleAsync(
				cancellationToken: TestContext.Current!.Execution.CancellationToken);

			// ReSharper disable once DisposeOnUsingVariable
			timer.Dispose();

			await That(async () => await wait).DoesNotThrow()
				.Because("disposing the last timer that advances the clock makes the timers idle");
		}
		finally
		{
			timeSystem.Scheduler.EndCreatingTimer();
		}
	}

	[Test]
	public async Task WaitUntilIdleAsync_WhenCanceled_ShouldThrowOperationCanceledException()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), 1.Seconds());
		using CancellationTokenSource cts = new();

		Task wait = timeSystem.TimerHandler.WaitUntilIdleAsync(Timeout.Infinite, cts.Token);
		cts.Cancel();

		await That(async () => await wait).Throws<OperationCanceledException>();
	}

	[Test]
	public async Task WaitUntilIdleAsync_WithAPeriodicTimerAndAutoAdvance_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 1.Seconds(), 1.Seconds());

		async Task Act() => await timeSystem.TimerHandler.WaitUntilIdleAsync(100,
			TestContext.Current!.Execution.CancellationToken);

		await That(Act).ThrowsExactly<TimeoutException>()
			.WithMessage("The timers did not become idle in 100ms: *").AsWildcard();
	}

	[Test]
	public async Task WaitUntilIdleAsync_WithInvalidTimeout_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();

		async Task Act() => await timeSystem.TimerHandler.WaitUntilIdleAsync(-2,
			TestContext.Current!.Execution.CancellationToken);

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("timeout");
	}
}
