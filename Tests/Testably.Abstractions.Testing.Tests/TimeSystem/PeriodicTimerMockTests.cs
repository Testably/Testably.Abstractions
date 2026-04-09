#if FEATURE_PERIODIC_TIMER
using aweXpect.Chronology;
using System.Threading;
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
}
#endif
