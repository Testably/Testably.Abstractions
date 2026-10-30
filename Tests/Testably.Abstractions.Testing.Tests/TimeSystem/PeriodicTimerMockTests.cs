#if FEATURE_PERIODIC_TIMER
using aweXpect.Chronology;
using System.Collections.ObjectModel;
using System.Threading;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class PeriodicTimerMockTests
{
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
				using IPeriodicTimer timer = timeSystem.PeriodicTimer.New(1.Seconds());
				timeSystem.TimeProvider.AdvanceBy(1.Seconds());
				while (await timer.WaitForNextTickAsync(token))
				{
					// ReSharper disable once AccessToModifiedClosure
					Interlocked.Increment(ref timerCount);
					// ReSharper disable AccessToDisposedClosure
					ticks.Release();
					await advanced.WaitAsync(token);
					// ReSharper restore AccessToDisposedClosure
				}
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

		// Changing the wall clock time should not affect the periodic timer
		timeSystem.TimeProvider.SetTo(timeSystem.DateTime.Now - 10.Seconds());

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
	public async Task
		WaitForNextTickAsync_DisableAutoAdvance_WhenClockAdvancesWhileStartingToWait_ShouldTick()
	{
		InterleavingTimeProviderFactory factory = new(
			new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		MockTimeSystem timeSystem = new(factory, o => o.DisableAutoAdvance());
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(5.Seconds());
		CancellationToken token = cts.Token;
		using IPeriodicTimer timer = timeSystem.PeriodicTimer.New(30.Seconds());
		using IAwaitableCallback<IPeriodicTimer> onWaiting = timeSystem.On.PeriodicTimer
			.WaitingForNextTick(_ => factory.Provider!.AdvanceOnNextRead = 30.Seconds());
		long start = timeSystem.TimeProvider.ElapsedTicks;

		bool result = false;
		async Task Act()
			=> result = await timer.WaitForNextTickAsync(token);

		await That(Act).DoesNotThrow()
			.Because("the clock was advanced to the next tick while the timer started to wait");
		await That(result).IsTrue();
		await That(timeSystem.TimeProvider.ElapsedTicks - start)
			.IsEqualTo(TimeSpan.FromSeconds(30).Ticks)
			.Because("the clock was advanced by exactly one period");
	}

	[Test]
	public async Task WaitForNextTickCount_AfterDispose_ShouldStopIncreasing()
	{
		MockTimeSystem timeSystem = new();
		IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());

		await periodicTimer.WaitForNextTickAsync();
		await periodicTimer.WaitForNextTickAsync();
		long countBeforeDispose = ((IPeriodicTimerMock)periodicTimer).WaitForNextTickCount;
		periodicTimer.Dispose();
		#pragma warning disable MA0040
		await periodicTimer.WaitForNextTickAsync();
		#pragma warning restore MA0040

		await That(((IPeriodicTimerMock)periodicTimer).WaitForNextTickCount)
			.IsEqualTo(countBeforeDispose);
	}

	[Test]
	public async Task WaitForNextTickCount_ShouldExposeMockAsIPeriodicTimerMock()
	{
		MockTimeSystem timeSystem = new();
		using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());

		await That(periodicTimer is IPeriodicTimerMock).IsTrue();
	}

	[Test]
	public async Task WaitForNextTickCount_ShouldIncrementOnEachCall()
	{
		MockTimeSystem timeSystem = new();
		using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
		IPeriodicTimerMock mock = (IPeriodicTimerMock)periodicTimer;

		await That(mock.WaitForNextTickCount).IsEqualTo(0L);

		await periodicTimer.WaitForNextTickAsync();
		await That(mock.WaitForNextTickCount).IsEqualTo(1L);

		await periodicTimer.WaitForNextTickAsync();
		await periodicTimer.WaitForNextTickAsync();
		await That(mock.WaitForNextTickCount).IsEqualTo(3L);
	}

	[Test]
	public async Task WaitForNextTickCount_WhenCancelledBeforeNotification_ShouldNotIncrement()
	{
		MockTimeSystem timeSystem = new();
		using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
		IPeriodicTimerMock mock = (IPeriodicTimerMock)periodicTimer;
		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = null;
		try
		{
			await periodicTimer.WaitForNextTickAsync(cts.Token);
		}
		catch (OperationCanceledException ex)
		{
			exception = ex;
		}

		await That(exception).IsNotNull();
		await That(mock.WaitForNextTickCount).IsEqualTo(0L);
	}

	[Test]
	public async Task WaitingForNextTick_AutoAdvance_ShouldFireCallback()
	{
		MockTimeSystem timeSystem = new();
		IPeriodicTimer? receivedTimer = null;
		using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());

		using (timeSystem.On.PeriodicTimer.WaitingForNextTick(t => receivedTimer = t))
		{
			await periodicTimer.WaitForNextTickAsync();
		}

		await That(receivedTimer).IsEqualTo(periodicTimer);
	}

	[Test]
	public async Task WaitingForNextTick_DisableAutoAdvance_ShouldFireCallbackBeforeWaiting()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(30.Seconds());
		CancellationToken token = cts.Token;

		int callbackCount = 0;
		using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
		using SemaphoreSlim callbackFired = new(0);

		using (timeSystem.On.PeriodicTimer.WaitingForNextTick(_ =>
			{
				callbackCount++;
				// ReSharper disable once AccessToDisposedClosure
				callbackFired.Release();
			}))
		{
			Task tickTask = Task.Run(async () =>
			{
				try
				{
					// ReSharper disable once AccessToDisposedClosure
					await periodicTimer.WaitForNextTickAsync(token);
				}
				catch (OperationCanceledException)
				{
					// Ignore cancellation
				}
			}, token);

			await callbackFired.WaitAsync(token);
			await That(callbackCount).IsEqualTo(1);

			timeSystem.TimeProvider.AdvanceBy(1.Seconds());
			await tickTask;
		}
	}

	[Test]
	public async Task WaitingForNextTick_ShouldFireCallbackForEachTick()
	{
		MockTimeSystem timeSystem = new();
		int callbackCount = 0;
		using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());

		using (timeSystem.On.PeriodicTimer.WaitingForNextTick(_ => callbackCount++))
		{
			await periodicTimer.WaitForNextTickAsync();
			await periodicTimer.WaitForNextTickAsync();
			await periodicTimer.WaitForNextTickAsync();
		}

		await That(callbackCount).IsEqualTo(3);
	}

	[Test]
	public async Task WaitingForNextTick_ShouldNotFireCallbackAfterDispose()
	{
		MockTimeSystem timeSystem = new();
		IPeriodicTimer? receivedTimer = null;
		using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
		IDisposable disposable =
			timeSystem.On.PeriodicTimer.WaitingForNextTick(t => receivedTimer = t);

		disposable.Dispose();
		await periodicTimer.WaitForNextTickAsync();

		await That(receivedTimer).IsNull();
	}

	[Test]
	public async Task WaitingForNextTick_WhenTimerIsDisposed_ShouldNotFireCallback()
	{
		MockTimeSystem timeSystem = new();
		IPeriodicTimer? receivedTimer = null;
		IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
		periodicTimer.Dispose();

		using (timeSystem.On.PeriodicTimer.WaitingForNextTick(t => receivedTimer = t))
		{
			#pragma warning disable MA0040 // Use an overload with a CancellationToken
			await periodicTimer.WaitForNextTickAsync();
			#pragma warning restore MA0040
		}

		await That(receivedTimer).IsNull();
	}

	private sealed class InterleavingTimeProviderFactory(DateTime start) : ITimeProviderFactory
	{
		public InterleavingTimeProvider? Provider { get; private set; }

		public ITimeProvider Create(Action<DateTime> onTimeChanged)
			=> Provider = new InterleavingTimeProvider(
				TimeProviderFactory.Use(start).Create(onTimeChanged));
	}

	/// <summary>
	///     Simulates another thread advancing the clock right after the next read of
	///     <see cref="ElapsedTicks" />.
	/// </summary>
	private sealed class InterleavingTimeProvider(ITimeProvider inner) : ITimeProvider
	{
		public TimeSpan? AdvanceOnNextRead { get; set; }

		public long ElapsedTicks
		{
			get
			{
				long value = inner.ElapsedTicks;
				if (AdvanceOnNextRead is { } interval)
				{
					AdvanceOnNextRead = null;
					inner.AdvanceBy(interval);
				}

				return value;
			}
		}

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

		public void AdvanceBy(TimeSpan interval)
			=> inner.AdvanceBy(interval);

		public TimeZoneInfo FindSystemTimeZoneById(string id)
			=> inner.FindSystemTimeZoneById(id);

		public ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
			=> inner.GetSystemTimeZones();

		public DateTime Read()
			=> inner.Read();

		public void RegisterTimeZone(TimeZoneInfo timeZone)
			=> inner.RegisterTimeZone(timeZone);

		public void SetTo(DateTime value)
			=> inner.SetTo(value);
	}
}
#endif
