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

		await That(observedTime).IsEqualTo(before + 2.Seconds())
			.Because("the callback observed the advanced mock clock through the bridge");
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
