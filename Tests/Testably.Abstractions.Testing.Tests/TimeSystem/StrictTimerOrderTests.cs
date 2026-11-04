using aweXpect.Chronology;
using System.Collections.Generic;
using System.Threading;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class StrictTimerOrderTests
{
	[Test]
	public async Task EnableStrictTimerOrder_WhenACallbackBlocksLongerThanOneSecond_ShouldHoldTheClock()
	{
		MockTimeSystem timeSystem = new(o => o.EnableStrictTimerOrder());
		List<string> events = [];
		using ManualResetEventSlim gate = new();
		using ITimer first = timeSystem.Timer.New(_ =>
		{
			gate.Wait();
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
		}, null, 2.Seconds(), Timeout.InfiniteTimeSpan);
		Thread releaser = new(() =>
		{
			Thread.Sleep(1500);
			gate.Set();
		});

		releaser.Start();
		timeSystem.TimerHandler.WaitUntilIdle();
		releaser.Join();

		await That(events).IsEqualTo(["first", "second",])
			.Because("with a strict timer order, the clock waits for a blocked callback beyond one second");
	}

	[Test]
	public async Task EnableStrictTimerOrder_WhenACallbackDoesNotComplete_ShouldFailEveryMoveOfTheClock()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance().EnableStrictTimerOrder(200.Milliseconds()));
		DateTime start = timeSystem.DateTime.UtcNow;
		using ManualResetEventSlim gate = new();
		using ITimer timer = timeSystem.Timer.New(_ => gate.Wait(), null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		try
		{
			void Act() => timeSystem.TimeProvider.AdvanceBy(2.Seconds());
			void ActAgain() => timeSystem.TimeProvider.AdvanceBy(1.Seconds());
			void SetTime() => timeSystem.TimeProvider.SetTo(start);

			await That(Act).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at * did not complete in 00:00:00.2000000 *")
				.AsWildcard();
			await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 1.Seconds())
				.Because("the clock must not move past the blocked callback");
			await That(ActAgain).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at *").AsWildcard();
			await That(SetTime).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at *").AsWildcard();
		}
		finally
		{
			gate.Set();
		}
	}

	[Test]
	public async Task EnableStrictTimerOrder_WhenACallbackOfAnAdvancingTimerDoesNotComplete_ShouldFailTheWaitUntilIdle()
	{
		MockTimeSystem timeSystem = new(o => o.EnableStrictTimerOrder(200.Milliseconds()));
		using ManualResetEventSlim gate = new();
		using ITimer timer = timeSystem.Timer.New(_ => gate.Wait(), null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		try
		{
			void Act() => timeSystem.TimerHandler.WaitUntilIdle(10000);

			await That(Act).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at *").AsWildcard()
				.Because("the scheduler failed in the background, which the wait reports instead of its own timeout");
		}
		finally
		{
			gate.Set();
		}
	}

	[Test]
	public async Task EnableStrictTimerOrder_WhenACallbackDelays_ShouldFireTheOtherTimersOnTheWay()
	{
		MockTimeSystem timeSystem = new(o => o.EnableStrictTimerOrder(5.Seconds()));
		DateTime start = timeSystem.DateTime.UtcNow;
		List<(string, TimeSpan)> events = [];
		List<ITimer> timers = [];

		void Add(string name)
		{
			lock (events)
			{
				events.Add((name, timeSystem.DateTime.UtcNow - start));
			}
		}

		using ITimer starter = timeSystem.Timer.New(_ =>
		{
			timers.Add(timeSystem.Timer.New(_ =>
			{
				timeSystem.Task.Delay(5.Seconds()).Wait();
				Add("delayed");
			}, null, 1.Seconds(), Timeout.InfiniteTimeSpan));
			timers.Add(timeSystem.Timer.New(_ => Add("other"), null, 3.Seconds(), Timeout.InfiniteTimeSpan));
		}, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

		timeSystem.TimerHandler.WaitUntilIdle();
		timers.ForEach(timer => timer.Dispose());

		await That(events).IsEqualTo([("other", 3.Seconds()), ("delayed", 6.Seconds()),])
			.Because("a delay in a callback moves the clock and fires the timers on the way, without waiting for itself");
	}

	[Test]
	[Arguments(0)]
	[Arguments(-2)]
	public async Task EnableStrictTimerOrder_WithInvalidCallbackTimeout_ShouldThrowArgumentOutOfRangeException(
		int milliseconds)
	{
		void Act() => _ = new MockTimeSystem(o => o.EnableStrictTimerOrder(TimeSpan.FromMilliseconds(milliseconds)));

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("callbackTimeout");
	}

	[Test]
	public async Task EnableStrictTimerOrder_WithInfiniteCallbackTimeout_ShouldNotThrow()
	{
		void Act() => _ = new MockTimeSystem(o => o.EnableStrictTimerOrder(Timeout.InfiniteTimeSpan));

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task EnableStrictTimerOrder_AfterABackgroundFailure_ShouldFailTheNextDelay()
	{
		MockTimeSystem timeSystem = new(o => o.EnableStrictTimerOrder(200.Milliseconds()));
		using ManualResetEventSlim gate = new();
		using ITimer timer = timeSystem.Timer.New(_ => gate.Wait(), null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		try
		{
			void WaitForTheFailure() => timeSystem.TimerHandler.WaitUntilIdle();
			void Act() => timeSystem.Task.Delay(1.Seconds());

			await That(WaitForTheFailure).ThrowsExactly<TimeoutException>();
			await That(Act).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at *").AsWildcard()
				.Because("the failure of the background loop stays, so that the next move of the clock reports it");
		}
		finally
		{
			gate.Set();
		}
	}

	[Test]
	public async Task EnableStrictTimerOrder_WhenAdvanceToNextTimerTimesOut_ShouldFailEveryLaterMoveOfTheClock()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance().EnableStrictTimerOrder());
		using ManualResetEventSlim gate = new();
		using ITimer timer = timeSystem.Timer.New(_ => gate.Wait(), null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		try
		{
			void Act() => timeSystem.TimerHandler.AdvanceToNextTimer(100);
			void ActAgain() => timeSystem.TimerHandler.AdvanceToNextTimer();

			await That(Act).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at * did not complete in 00:00:00.1000000 *").AsWildcard();
			await That(ActAgain).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at *").AsWildcard()
				.Because("no timer is left, but the clock must not pretend that nothing happened");
		}
		finally
		{
			gate.Set();
		}
	}

	[Test]
	public async Task EnableStrictTimerOrder_WithTooLargeCallbackTimeout_ShouldThrowArgumentOutOfRangeException()
	{
		void Act() => _ = new MockTimeSystem(o => o.EnableStrictTimerOrder(TimeSpan.MaxValue));

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("callbackTimeout");
	}

#if FEATURE_TIMEPROVIDER
	[Test]
	public async Task EnableStrictTimerOrder_WhenATimeoutGuardsADelayOnTheTimeProvider_ShouldCancelTheDelayAtTheTimeout()
	{
		MockTimeSystem timeSystem = new(o => o.EnableStrictTimerOrder());
		System.TimeProvider timeProvider = timeSystem.ToTimeProvider();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new(10.Seconds(), timeProvider);

		await That(() => Task.Delay(30.Seconds(), timeProvider, cts.Token)).Throws<TaskCanceledException>();
		await That(timeSystem.DateTime.UtcNow - start).IsEqualTo(10.Seconds());
	}

	[Test]
	public async Task EnableStrictTimerOrder_WhenTheCreatorOfATimerDoesNotSetItUp_ShouldMoveOnAfterTheTimeout()
	{
		MockTimeSystem timeSystem = new(o => o.EnableStrictTimerOrder(200.Milliseconds()));
		System.TimeProvider timeProvider = timeSystem.ToTimeProvider();
		CancellationTokenSource cts = new();
		CancellationToken token = cts.Token;
		cts.Dispose();

		// The delay never registers the token of the disposed source, so it never finishes setting up its timer.
		Task delay = Task.Delay(5.Seconds(), timeProvider, token);
		await timeSystem.TimerHandler.WaitUntilIdleAsync(
			cancellationToken: TestContext.Current!.Execution.CancellationToken);

		await That(delay.IsCompleted).IsTrue()
			.Because("like on the real system, a delay with the token of a disposed source completes");
	}
#endif

	[Test]
	public async Task EnableStrictTimerOrder_WhenACallbackOfAnAdvancingTimerDoesNotComplete_ShouldFailTheWaitUntilIdleAsync()
	{
		MockTimeSystem timeSystem = new(o => o.EnableStrictTimerOrder(200.Milliseconds()));
		using ManualResetEventSlim gate = new();
		using ITimer timer = timeSystem.Timer.New(_ => gate.Wait(), null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		try
		{
			async Task Act() => await timeSystem.TimerHandler.WaitUntilIdleAsync(10000,
				TestContext.Current!.Execution.CancellationToken);
			async Task ActAgain() => await timeSystem.TimerHandler.WaitUntilIdleAsync(10000,
				TestContext.Current!.Execution.CancellationToken);

			await That(Act).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at *").AsWildcard();
			await That(ActAgain).ThrowsExactly<TimeoutException>()
				.WithMessage("The callback of the timer due at *").AsWildcard()
				.Because("a later wait reports the failure, also when it finds the timers completed at once");
		}
		finally
		{
			gate.Set();
		}
	}
}
