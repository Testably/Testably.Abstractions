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
}
#endif
