using System.Collections.Generic;
using System.Threading;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Tests.TimeSystem;

#pragma warning disable TUnit0031
[TimeSystemTests]
public class TimerTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	#region Test Setup

	private const int TimerMultiplier = 10;

	#endregion

#if NET8_0_OR_GREATER
	[Test]
	public async Task Change_DisposedTimer_ShouldReturnFalse()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 0, 200);
		// ReSharper disable once DisposeOnUsingVariable
		timer.Dispose();
		bool result = true;
		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			result = timer.Change(100, 200);
		}

		await That(Act).DoesNotThrow();
		await That(result).IsFalse();
	}
#endif

#if !NET8_0_OR_GREATER
	[Test]
	public async Task Change_DisposedTimer_ShouldThrowObjectDisposedException()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 0, 200);
		// ReSharper disable once DisposeOnUsingVariable
		timer.Dispose();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			_ = timer.Change(100, 200);
		}

		await That(Act).ThrowsExactly<ObjectDisposedException>();
	}
#endif

	[Test]
	public async Task Change_Infinite_ShouldBeValidDueTime()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(Timeout.Infinite, 0);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task Change_Infinite_ShouldBeValidPeriod()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, Timeout.Infinite);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	[Arguments(-2)]
	[Arguments(-500)]
	public async Task Change_InvalidDueTime_ShouldThrowArgumentOutOfRangeException(int dueTime)
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(dueTime, 0);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086).And
			.WithParamName(nameof(dueTime));
	}

	[Test]
	[Arguments(-2)]
	[Arguments(-500)]
	public async Task Change_InvalidPeriod_ShouldThrowArgumentOutOfRangeException(int period)
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, period);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086).And
			.WithParamName(nameof(period));
	}

	[Test]
	public async Task Change_SameValues_WithInt_ShouldReturnTrue()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		bool result = timer.Change(100, 200);

		await That(result).IsTrue();
	}

	[Test]
	public async Task Change_SameValues_WithLong_ShouldReturnTrue()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100L, 200L);

		bool result = timer.Change(100L, 200L);

		await That(result).IsTrue();
	}

	[Test]
	public async Task Change_SameValues_WithTimeSpan_ShouldReturnTrue()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));

		bool result = timer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));

		await That(result).IsTrue();
	}

	[Test]
	[Skip("Temporarily skip brittle tests")]
	public async Task Change_WithInt_ShouldResetTimer()
	{
		SkipIfBrittleTestsShouldBeSkipped();

		List<int> triggerTimes = [];
		DateTime previousTime = TimeSystem.DateTime.Now;
		using ManualResetEventSlim ms1 = new();
		using ManualResetEventSlim ms2 = new();
		using ManualResetEventSlim ms3 = new();
		#pragma warning disable MA0147 // Avoid async void method for delegate
		// ReSharper disable once AsyncVoidLambda
		using (ITimer timer1 = TimeSystem.Timer.New(async _ =>
				{
					// ReSharper disable once AccessToDisposedClosure
					try
					{
						DateTime now = TimeSystem.DateTime.Now;
						double diff = (now - previousTime).TotalMilliseconds;
						previousTime = now;
						ms1.Set();
						triggerTimes.Add((int)diff);
						// ReSharper disable once AccessToDisposedClosure
						await That(ms2.Wait(ExpectSuccess, CancellationToken))
							.IsTrue();
						if (triggerTimes.Count > 3)
						{
							// ReSharper disable once CancellationToken
							ms3.Set();
						}

						await Task.Delay(10, CancellationToken);
					}
					catch (ObjectDisposedException)
					{
						// Ignore any ObjectDisposedException
					}
				},
				null, (long)(0 * TimerMultiplier), 200 * TimerMultiplier))
			#pragma warning restore MA0147 // Avoid async void method for delegate
		{
			await That(ms1.Wait(ExpectSuccess, CancellationToken)).IsTrue();
			using ITimer timer2 = TimeSystem.Timer.New(_ =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					timer1.Change(0 * TimerMultiplier, 200 * TimerMultiplier);
					// ReSharper disable once AccessToDisposedClosure
					ms2.Set();
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, null, 100 * TimerMultiplier, 0 * TimerMultiplier);

			await That(ms3.Wait(ExpectSuccess * TimerMultiplier,
				CancellationToken)).IsTrue();
		}

		if (triggerTimes[0] < 30 * TimerMultiplier)
		{
			triggerTimes.RemoveAt(0);
		}

		await That(triggerTimes[0]).IsLessThan(130 * TimerMultiplier);
		for (int i = 1; i < triggerTimes.Count; i++)
		{
			await That(triggerTimes[i]).IsGreaterThan(170 * TimerMultiplier).And
				.IsLessThan(230 * TimerMultiplier);
		}
	}

	[Test]
	[Skip("Temporarily skip brittle tests")]
	public async Task Change_WithLong_ShouldResetTimer()
	{
		SkipIfBrittleTestsShouldBeSkipped();

		List<int> triggerTimes = [];
		DateTime previousTime = TimeSystem.DateTime.Now;
		using ManualResetEventSlim ms1 = new();
		using ManualResetEventSlim ms2 = new();
		using ManualResetEventSlim ms3 = new();
		#pragma warning disable MA0147 // Avoid async void method for delegate
		// ReSharper disable once AsyncVoidLambda
		using (ITimer timer1 = TimeSystem.Timer.New(async _ =>
				{
					// ReSharper disable once AccessToDisposedClosure
					try
					{
						DateTime now = TimeSystem.DateTime.Now;
						double diff = (now - previousTime).TotalMilliseconds;
						previousTime = now;
						ms1.Set();
						triggerTimes.Add((int)diff);
						// ReSharper disable once AccessToDisposedClosure
						await That(ms2.Wait(ExpectSuccess, CancellationToken))
							.IsTrue();
						if (triggerTimes.Count > 3)
						{
							// ReSharper disable once AccessToDisposedClosure
							ms3.Set();
						}

						await Task.Delay(10, CancellationToken);
					}
					catch (ObjectDisposedException)
					{
						// Ignore any ObjectDisposedException
					}
				},
				null, 0L * TimerMultiplier, 200L * TimerMultiplier))
			#pragma warning restore MA0147 // Avoid async void method for delegate
		{
			await That(ms1.Wait(ExpectSuccess, CancellationToken)).IsTrue();
			using ITimer timer2 = TimeSystem.Timer.New(_ =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					timer1.Change(0L * TimerMultiplier, 200L * TimerMultiplier);
					// ReSharper disable once AccessToDisposedClosure
					ms2.Set();
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, null, 100L * TimerMultiplier, 0L * TimerMultiplier);

			await That(ms3.Wait(ExpectSuccess, CancellationToken)).IsTrue();
		}

		if (triggerTimes[0] < 30 * TimerMultiplier)
		{
			triggerTimes.RemoveAt(0);
		}

		await That(triggerTimes[0]).IsLessThan(130 * TimerMultiplier);
		for (int i = 1; i < triggerTimes.Count; i++)
		{
			await That(triggerTimes[i]).IsGreaterThan(170 * TimerMultiplier).And
				.IsLessThan(230 * TimerMultiplier);
		}
	}

	[Test]
	[Skip("Temporarily skip brittle tests")]
	public async Task Change_WithTimeSpan_ShouldResetTimer()
	{
		SkipIfBrittleTestsShouldBeSkipped();

		List<int> triggerTimes = [];
		DateTime previousTime = TimeSystem.DateTime.Now;
		using ManualResetEventSlim ms1 = new();
		using ManualResetEventSlim ms2 = new();
		using ManualResetEventSlim ms3 = new();
		#pragma warning disable MA0147 // Avoid async void method for delegate
		// ReSharper disable once AsyncVoidLambda
		using (ITimer timer1 = TimeSystem.Timer.New(async _ =>
				{
					// ReSharper disable once AccessToDisposedClosure
					try
					{
						DateTime now = TimeSystem.DateTime.Now;
						double diff = (now - previousTime).TotalMilliseconds;
						previousTime = now;
						ms1.Set();
						triggerTimes.Add((int)diff);
						// ReSharper disable once AccessToDisposedClosure
						await That(ms2.Wait(ExpectSuccess, CancellationToken))
							.IsTrue();
						if (triggerTimes.Count > 3)
						{
							// ReSharper disable once AccessToDisposedClosure
							ms3.Set();
						}

						await Task.Delay(10, CancellationToken);
					}
					catch (ObjectDisposedException)
					{
						// Ignore any ObjectDisposedException
					}
				}, null, TimeSpan.FromMilliseconds(TimerMultiplier),
				#pragma warning restore TUnit0031
				TimeSpan.FromMilliseconds(200 * TimerMultiplier)))
			#pragma warning restore MA0147 // Avoid async void method for delegate
		{
			await That(ms1.Wait(ExpectSuccess, CancellationToken)).IsTrue();
			using ITimer timer2 = TimeSystem.Timer.New(_ =>
				{
					// ReSharper disable once AccessToDisposedClosure
					try
					{
						timer1.Change(TimeSpan.FromMilliseconds(TimerMultiplier),
							TimeSpan.FromMilliseconds(200 * TimerMultiplier));
						// ReSharper disable once AccessToDisposedClosure
						ms2.Set();
					}
					catch (ObjectDisposedException)
					{
						// Ignore any ObjectDisposedException
					}
				}, null, TimeSpan.FromMilliseconds(100 * TimerMultiplier),
				TimeSpan.FromMilliseconds(TimerMultiplier));

			await That(ms3.Wait(ExpectSuccess, CancellationToken)).IsTrue();
		}

		if (triggerTimes[0] < 30 * TimerMultiplier)
		{
			triggerTimes.RemoveAt(0);
		}

		await That(triggerTimes[0]).IsLessThan(130 * TimerMultiplier);
		for (int i = 1; i < triggerTimes.Count; i++)
		{
			await That(triggerTimes[i]).IsGreaterThan(170 * TimerMultiplier).And
				.IsLessThan(230 * TimerMultiplier);
		}
	}

	[Test]
	public async Task Dispose_WithManualResetEventWaitHandle_ShouldBeSet()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using ManualResetEvent waitHandle = new(false);
		bool result = timer.Dispose(waitHandle);

		await That(waitHandle.WaitOne(1000)).IsTrue();
		await That(result).IsTrue();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		}

#if NET8_0_OR_GREATER
		await That(Act).DoesNotThrow();
#else
		await That(Act).ThrowsExactly<ObjectDisposedException>();
#endif
	}

	[Test]
	public async Task Dispose_WithMutexWaitHandle_ShouldBeSet()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using Mutex waitHandle = new(false);
		bool result = timer.Dispose(waitHandle);

		await That(waitHandle.WaitOne(1000)).IsTrue();
		await That(result).IsTrue();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		}

#if NET8_0_OR_GREATER
		await That(Act).DoesNotThrow();
#else
		await That(Act).ThrowsExactly<ObjectDisposedException>();
#endif
	}

	[Test]
	public async Task Dispose_WithSemaphoreWaitHandle_ShouldBeSet()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using Semaphore waitHandle = new(1, 1);
		bool result = timer.Dispose(waitHandle);

		await That(waitHandle.WaitOne(1000)).IsTrue();
		await That(result).IsTrue();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		}

#if NET8_0_OR_GREATER
		await That(Act).DoesNotThrow();
#else
		await That(Act).ThrowsExactly<ObjectDisposedException>();
#endif
	}

	[Test]
	public async Task Dispose_WithWaitHandleCalledTwice_ShouldReturnFalse()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using ManualResetEvent waitHandle = new(false);
		timer.Dispose(waitHandle);

		bool result = timer.Dispose(waitHandle);

		await That(result).IsFalse();
	}

#if FEATURE_ASYNC_DISPOSABLE
	[Test]
	public async Task DisposeAsync_ShouldDisposeTimer()
	{
		await using ITimer timer = TimeSystem.Timer.New(_ =>
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
		await That(Act).ThrowsExactly<ObjectDisposedException>();
#endif
	}
#endif
}
#pragma warning restore TUnit0031
