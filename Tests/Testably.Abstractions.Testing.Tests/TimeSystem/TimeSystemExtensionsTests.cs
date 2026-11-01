#if FEATURE_TIMEPROVIDER
using aweXpect.Chronology;
using System.Threading;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimeSystemExtensionsTests
{
	[Test]
	public async Task ToTimeProvider_AdvanceBy_ShouldBeObservableThroughGetUtcNow()
	{
		DateTime now = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(now);
		System.TimeProvider provider = timeSystem.ToTimeProvider();

		DateTimeOffset before = provider.GetUtcNow();
		timeSystem.TimeProvider.AdvanceBy(TimeSpan.FromHours(1));
		DateTimeOffset after = provider.GetUtcNow();

		await That(after - before).IsEqualTo(TimeSpan.FromHours(1));
	}

	[Test]
	public async Task ToTimeProvider_CancellationTokenSourceWithDelay_ShouldCancelPeriodicTimerWaitOfTheMockAtItsDueTime()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new(1.Seconds(), timeSystem.ToTimeProvider());
		using Testably.Abstractions.TimeSystem.IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(5.Seconds());

		async Task Act() => await periodicTimer.WaitForNextTickAsync(cts.Token);

		await That(Act).Throws<OperationCanceledException>();
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 1.Seconds())
			.Because("the timeout canceled the wait after one second");
		await That(await periodicTimer.WaitForNextTickAsync(TestContext.Current!.Execution.CancellationToken))
			.IsTrue();
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 5.Seconds())
			.Because("the cancellation does not shift the ticks of the periodic timer");
	}

	[Test]
	public async Task ToTimeProvider_CancellationTokenSourceWithDelay_ShouldCancelTaskDelayOfTheMockAtItsDueTime()
	{
		MockTimeSystem timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new(1.Seconds(), timeSystem.ToTimeProvider());

		Task delay = timeSystem.Task.Delay(5.Seconds(), cts.Token);

		await That(delay.IsCanceled).IsTrue();
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 1.Seconds())
			.Because("the timeout canceled the delay after one second");
	}

	[Test]
	public async Task ToTimeProvider_CancellationTokenSourceWithDelay_ShouldNotMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		DateTimeOffset before = provider.GetUtcNow();

		using CancellationTokenSource cts = new(10.Seconds(), provider);

		await That(timeSystem.Scheduler.IsAdvancing).IsFalse()
			.Because("the timer of a timeout does not move the clock on its own");
		await That(cts.IsCancellationRequested).IsFalse();
		await That(provider.GetUtcNow()).IsEqualTo(before);
	}

	[Test]
	public async Task ToTimeProvider_CancellationTokenSourceWithDelay_WhenASleepPassesIt_ShouldCancelAtItsDueTime()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		DateTime start = timeSystem.DateTime.UtcNow;
		using CancellationTokenSource cts = new(1.Seconds(), provider);
		DateTime? canceledAt = null;
		using CancellationTokenRegistration _ = cts.Token.Register(() => canceledAt = timeSystem.DateTime.UtcNow);

		timeSystem.Thread.Sleep(5.Seconds());

		await That(canceledAt).IsEqualTo(start + 1.Seconds());
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(start + 5.Seconds());
	}

	[Test]
	public async Task ToTimeProvider_CancellationTokenSourceWithDelay_WhenATimerWithoutDueTimeExists_ShouldNotCancel()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		DateTimeOffset before = provider.GetUtcNow();
		using CancellationTokenSource cts = new(10.Seconds(), provider);

		using System.Threading.ITimer timer = provider.CreateTimer(_ => { }, null,
			Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

		await That(timeSystem.Scheduler.IsAdvancing).IsFalse()
			.Because("a timer that is not armed does not wait for anything");
		await That(cts.IsCancellationRequested).IsFalse();
		await That(provider.GetUtcNow()).IsEqualTo(before);
	}

	[Test]
	public async Task ToTimeProvider_CancellationTokenSourceWithDelay_WhenUnrelated_ShouldNotStallAutoAdvancingTimers()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		using CancellationTokenSource cts = new(1500.Milliseconds(), provider);
		TaskCompletionSource<bool> executedThreeTimes = new(TaskCreationOptions.RunContinuationsAsynchronously);
		int executions = 0;

		using Testably.Abstractions.TimeSystem.ITimer timer = timeSystem.Timer.New(_ =>
		{
			if (Interlocked.Increment(ref executions) == 3)
			{
				executedThreeTimes.TrySetResult(true);
			}
		}, null, 1.Seconds(), 1.Seconds());

		bool isCompleted = await Task.WhenAny(executedThreeTimes.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == executedThreeTimes.Task;

		await That(isCompleted).IsTrue()
			.Because("a timeout that the timer passes does not belong to it");
		await That(cts.IsCancellationRequested).IsTrue();
	}

	[Test]
	public async Task ToTimeProvider_Change_ShouldRescheduleTimer()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		CancellationToken token = CreateTimeout(out CancellationTokenSource cts);
		using CancellationTokenSource _ = cts;
		using SemaphoreSlim callbackExecuted = new(0);

		using ITimer timer = provider.CreateTimer(
			// ReSharper disable once AccessToDisposedClosure
			_ => callbackExecuted.Release(),
			null,
			Timeout.InfiniteTimeSpan,
			Timeout.InfiniteTimeSpan);

		// Never fires while due time is infinite.
		timeSystem.TimeProvider.AdvanceBy(10.Seconds());
		await That(callbackExecuted.CurrentCount).IsEqualTo(0);

		bool changed = timer.Change(1.Seconds(), Timeout.InfiniteTimeSpan);
		await LetTimerSubscribe(token);
		timeSystem.TimeProvider.AdvanceBy(1.Seconds());
		await callbackExecuted.WaitAsync(token);

		await That(changed).IsTrue();
	}

	[Test]
	public async Task ToTimeProvider_CreateTimer_ShouldFireRepeatedlyForPeriodicTimer()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		CancellationToken token = CreateTimeout(out CancellationTokenSource cts);
		using CancellationTokenSource _ = cts;
		int count = 0;
		using SemaphoreSlim callbackExecuted = new(0);

		using ITimer timer = provider.CreateTimer(
			_ =>
			{
				Interlocked.Increment(ref count);
				// ReSharper disable once AccessToDisposedClosure
				callbackExecuted.Release();
			},
			null,
			1.Seconds(),
			1.Seconds());

		for (int i = 0; i < 3; i++)
		{
			await LetTimerSubscribe(token);
			timeSystem.TimeProvider.AdvanceBy(1.Seconds());
			await callbackExecuted.WaitAsync(token);
		}

		await That(count).IsGreaterThanOrEqualTo(3);
	}

	[Test]
	public async Task ToTimeProvider_CreateTimer_ShouldFireWhenClockAdvances()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(30.Seconds());
		CancellationToken token = cts.Token;
		using SemaphoreSlim callbackExecuted = new(0);
		DateTimeOffset observedTime = default;

		DateTimeOffset before = provider.GetUtcNow();
		using ITimer timer = provider.CreateTimer(
			_ =>
			{
				observedTime = provider.GetUtcNow();
				// ReSharper disable once AccessToDisposedClosure
				callbackExecuted.Release();
			},
			null,
			1.Seconds(),
			Timeout.InfiniteTimeSpan);

		#pragma warning disable MA0166 // Use an overload with a TimeProvider
		await Task.Delay(50.Milliseconds(), token);
		#pragma warning restore MA0166
		timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		await callbackExecuted.WaitAsync(token);

		await That(observedTime).IsEqualTo(before + 1.Seconds())
			.Because("the callback observed the mock clock at its due time through the bridge");
	}

	[Test]
	public async Task ToTimeProvider_CreateTimer_WhenTheCallbackWaitsForADelay_ShouldNotDeadlock()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		TaskCompletionSource<bool> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

		using ITimer timer = provider.CreateTimer(_ =>
		{
			Task.Delay(1.Seconds(), provider).Wait();
			completed.TrySetResult(true);
		}, null, 1.Seconds(), Timeout.InfiniteTimeSpan);

		bool isCompleted = await Task.WhenAny(completed.Task,
			Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken)) == completed.Task;

		await That(isCompleted).IsTrue()
			.Because("a callback that blocks on a delay of the same time provider must not keep the clock from moving");
	}

	[Test]
	public async Task ToTimeProvider_CreateTimer_WithACancellationTokenSourceAndAPeriod_ShouldMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		using CancellationTokenSource shutdown = new();
		TaskCompletionSource<bool> executedThreeTimes = new(TaskCreationOptions.RunContinuationsAsynchronously);
		int executions = 0;

		using ITimer timer = provider.CreateTimer(_ =>
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
	public async Task ToTimeProvider_Delay_ShouldMoveTheClock()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		DateTimeOffset before = provider.GetUtcNow();

		await Task.Delay(5.Seconds(), provider, TestContext.Current!.Execution.CancellationToken);

		await That(provider.GetUtcNow()).IsEqualTo(before + 5.Seconds());
	}

	[Test]
	public async Task ToTimeProvider_Delay_WhenATimeoutCancelsIt_ShouldStopTheClockAtTheTimeout()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		DateTimeOffset before = provider.GetUtcNow();
		using CancellationTokenSource cts = new(1.Seconds(), provider);

		Task Act() => Task.Delay(5.Seconds(), provider, cts.Token);

		await That(Act).Throws<OperationCanceledException>();
		await That(provider.GetUtcNow()).IsEqualTo(before + 1.Seconds())
			.Because("the timeout canceled the delay after one second");
	}

	[Test]
	public async Task ToTimeProvider_DisposeAsync_ShouldStopTimer()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		CancellationToken token = CreateTimeout(out CancellationTokenSource cts);
		using CancellationTokenSource _ = cts;
		int count = 0;

		ITimer timer = provider.CreateTimer(
			_ => Interlocked.Increment(ref count),
			null,
			1.Seconds(),
			1.Seconds());
		await timer.DisposeAsync();

		timeSystem.TimeProvider.AdvanceBy(5.Seconds());
		#pragma warning disable MA0166 // Use an overload with a TimeProvider
		// Give any (erroneously) scheduled callback a chance to run before asserting.
		await Task.Delay(100.Milliseconds(), token);
		#pragma warning restore MA0166

		await That(count).IsEqualTo(0);
	}

	[Test]
	public async Task ToTimeProvider_GetElapsedTime_ShouldMatchAdvancedInterval()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();

		long start = provider.GetTimestamp();
		timeSystem.TimeProvider.AdvanceBy(5.Seconds());
		TimeSpan elapsed = provider.GetElapsedTime(start);

		await That(elapsed).IsEqualTo(5.Seconds());
	}

	[Test]
	public async Task ToTimeProvider_GetLocalNow_ShouldMatchMockDateTimeNow()
	{
		DateTime now = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		timeSystem.TimeProvider.SetTo(now);
		System.TimeProvider provider = timeSystem.ToTimeProvider();

		DateTimeOffset localNow = provider.GetLocalNow();

		await That(localNow.DateTime).IsEqualTo(timeSystem.DateTime.Now)
			.Because("GetLocalNow uses the same local time zone as IDateTime.Now");
	}

	[Test]
	public async Task ToTimeProvider_GetUtcNow_ShouldReflectMockTime()
	{
		DateTime now = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(now);

		System.TimeProvider provider = timeSystem.ToTimeProvider();

		await That(provider.GetUtcNow()).IsEqualTo(new DateTimeOffset(now));
	}

	[Test]
	public async Task ToTimeProvider_Null_ShouldThrowArgumentNullException()
	{
		ITimeSystem? timeSystem = null;

		void Act() => timeSystem!.ToTimeProvider();

		await That(Act).ThrowsExactly<ArgumentNullException>()
			.WithParamName("timeSystem");
	}

	[Test]
	public async Task ToTimeProvider_RealTimeSystem_GetUtcNow_ShouldReturnCurrentTime()
	{
		RealTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();

		DateTimeOffset begin = DateTimeOffset.UtcNow;
		DateTimeOffset result = provider.GetUtcNow();
		DateTimeOffset end = DateTimeOffset.UtcNow;

		await That(result).IsOnOrAfter(begin).And.IsOnOrBefore(end);
	}

	[Test]
	public async Task ToTimeProvider_WaitAsync_ShouldNotTimeOutBeforeTheClockPassesTheTimeout()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		DateTimeOffset before = provider.GetUtcNow();
		TaskCompletionSource<bool> neverCompleted = new();

		Task waiting = neverCompleted.Task.WaitAsync(10.Seconds(), provider);

		await That(timeSystem.Scheduler.IsAdvancing).IsFalse()
			.Because("the timer of a timeout does not move the clock on its own");
		await That(waiting.IsCompleted).IsFalse();
		await That(provider.GetUtcNow()).IsEqualTo(before);
	}

	[Test]
	public async Task ToTimeProvider_WaitAsync_WhenADelayPassesTheTimeout_ShouldTimeOut()
	{
		MockTimeSystem timeSystem = new();
		System.TimeProvider provider = timeSystem.ToTimeProvider();
		TaskCompletionSource<bool> neverCompleted = new();
		Task waiting = neverCompleted.Task.WaitAsync(1.Seconds(), provider);

		await Task.Delay(5.Seconds(), provider, TestContext.Current!.Execution.CancellationToken);

		await That(() => waiting).ThrowsExactly<TimeoutException>();
	}

	private static CancellationToken CreateTimeout(out CancellationTokenSource cts)
	{
		cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(30.Seconds());
		return cts.Token;
	}

	private static async Task LetTimerSubscribe(CancellationToken token)
	{
		#pragma warning disable MA0166 // Use an overload with a TimeProvider
		// Real delay so the background timer can register its time-changed subscription
		// before the mock clock is advanced.
		await Task.Delay(50.Milliseconds(), token);
		#pragma warning restore MA0166
	}
}
#endif
