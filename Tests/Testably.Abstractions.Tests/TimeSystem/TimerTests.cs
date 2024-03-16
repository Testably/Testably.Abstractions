using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class TimerTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	#region Test Setup

	private const int TimerMultiplier = 10;

	#endregion

	[SkippableFact]
	public void Change_DisposedTimer_ShouldThrowObjectDisposedException()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 0, 200);
		timer.Dispose();
		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(100, 200);
		});

#if NET8_0_OR_GREATER
		exception.Should().BeNull();
#else
		exception.Should().BeOfType<ObjectDisposedException>();
#endif
	}

	[SkippableFact]
	public void Change_Infinite_ShouldBeValidDueTime()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(Timeout.Infinite, 0);
		});

		exception.Should().BeNull();
	}

	[SkippableFact]
	public void Change_Infinite_ShouldBeValidPeriod()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, Timeout.Infinite);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[InlineData(-2)]
	[InlineData(-500)]
	public void Change_InvalidDueTime_ShouldThrowArgumentOutOfRangeException(int dueTime)
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(dueTime, 0);
		});

		exception.Should()
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086,
				paramName: nameof(dueTime));
	}

	[SkippableTheory]
	[InlineData(-2)]
	[InlineData(-500)]
	public void Change_InvalidPeriod_ShouldThrowArgumentOutOfRangeException(int period)
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, period);
		});

		exception.Should()
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086,
				paramName: nameof(period));
	}

	[SkippableFact]
	public void Change_SameValues_WithInt_ShouldReturnTrue()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		bool result = timer.Change(100, 200);

		result.Should().BeTrue();
	}

	[SkippableFact]
	public void Change_SameValues_WithLong_ShouldReturnTrue()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100L, 200L);

		bool result = timer.Change(100L, 200L);

		result.Should().BeTrue();
	}

	[SkippableFact]
	public void Change_SameValues_WithTimeSpan_ShouldReturnTrue()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));

		bool result = timer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));

		result.Should().BeTrue();
	}

	[SkippableFact]
	public void Change_WithInt_ShouldResetTimer()
	{
		Test.SkipBrittleTestsOnRealTimeSystem(TimeSystem);

		List<int> triggerTimes = new();
		DateTime previousTime = TimeSystem.DateTime.Now;
		ManualResetEventSlim ms = new();
		ManualResetEventSlim ms2 = new();
		ManualResetEventSlim ms3 = new();
		// ReSharper disable once AsyncVoidLambda
		using ITimer timer1 = TimeSystem.Timer.New(async _ =>
			{
				DateTime now = TimeSystem.DateTime.Now;
				double diff = (now - previousTime).TotalMilliseconds;
				previousTime = now;
				ms.Set();
				triggerTimes.Add((int)diff);
				ms2.Wait(30000);
				if (triggerTimes.Count > 3)
				{
					ms3.Set();
				}

				await Task.Delay(10);
			},
			null, 0 * TimerMultiplier, 200 * TimerMultiplier);
		ms.Wait(30000).Should().BeTrue();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer1.Change(0 * TimerMultiplier, 200 * TimerMultiplier);
			ms2.Set();
		}, null, 100 * TimerMultiplier, 0 * TimerMultiplier);

		ms3.Wait(10000 * TimerMultiplier);

		timer1.Dispose();
		timer2.Dispose();
		if (triggerTimes[0] < 30 * TimerMultiplier)
		{
			triggerTimes.RemoveAt(0);
		}

		triggerTimes[0].Should()
			.BeGreaterThan(70 * TimerMultiplier).And
			.BeLessThan(130 * TimerMultiplier);
		for (int i = 1; i < triggerTimes.Count; i++)
		{
			triggerTimes[i].Should()
				.BeGreaterThan(170 * TimerMultiplier).And
				.BeLessThan(230 * TimerMultiplier);
		}
	}

	[SkippableFact]
	public void Change_WithLong_ShouldResetTimer()
	{
		Test.SkipBrittleTestsOnRealTimeSystem(TimeSystem);

		List<int> triggerTimes = new();
		DateTime previousTime = TimeSystem.DateTime.Now;
		ManualResetEventSlim ms = new();
		ManualResetEventSlim ms2 = new();
		ManualResetEventSlim ms3 = new();
		// ReSharper disable once AsyncVoidLambda
		using ITimer timer1 = TimeSystem.Timer.New(async _ =>
			{
				DateTime now = TimeSystem.DateTime.Now;
				double diff = (now - previousTime).TotalMilliseconds;
				previousTime = now;
				ms.Set();
				triggerTimes.Add((int)diff);
				ms2.Wait(30000);
				if (triggerTimes.Count > 3)
				{
					ms3.Set();
				}

				await Task.Delay(10);
			},
			null, 0L * TimerMultiplier, 200L * TimerMultiplier);
		ms.Wait(30000).Should().BeTrue();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer1.Change(0L * TimerMultiplier, 200L * TimerMultiplier);
			ms2.Set();
		}, null, 100L * TimerMultiplier, 0L * TimerMultiplier);

		ms3.Wait(10000);

		timer1.Dispose();
		timer2.Dispose();
		if (triggerTimes[0] < 30 * TimerMultiplier)
		{
			triggerTimes.RemoveAt(0);
		}

		triggerTimes[0].Should()
			.BeGreaterThan(70 * TimerMultiplier).And
			.BeLessThan(130 * TimerMultiplier);
		for (int i = 1; i < triggerTimes.Count; i++)
		{
			triggerTimes[i].Should()
				.BeGreaterThan(170 * TimerMultiplier).And
				.BeLessThan(230 * TimerMultiplier);
		}
	}

	[SkippableFact]
	public void Change_WithTimeSpan_ShouldResetTimer()
	{
		Test.SkipBrittleTestsOnRealTimeSystem(TimeSystem);

		List<int> triggerTimes = new();
		DateTime previousTime = TimeSystem.DateTime.Now;
		ManualResetEventSlim ms = new();
		ManualResetEventSlim ms2 = new();
		ManualResetEventSlim ms3 = new();
		// ReSharper disable once AsyncVoidLambda
		using ITimer timer1 = TimeSystem.Timer.New(async _ =>
			{
				DateTime now = TimeSystem.DateTime.Now;
				double diff = (now - previousTime).TotalMilliseconds;
				previousTime = now;
				ms.Set();
				triggerTimes.Add((int)diff);
				ms2.Wait(30000);
				if (triggerTimes.Count > 3)
				{
					ms3.Set();
				}

				await Task.Delay(10);
			}, null, TimeSpan.FromMilliseconds(0 * TimerMultiplier),
			TimeSpan.FromMilliseconds(200 * TimerMultiplier));
		ms.Wait(30000).Should().BeTrue();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
			{
				// ReSharper disable once AccessToDisposedClosure
				timer1.Change(TimeSpan.FromMilliseconds(0 * TimerMultiplier),
					TimeSpan.FromMilliseconds(200 * TimerMultiplier));
				ms2.Set();
			}, null, TimeSpan.FromMilliseconds(100 * TimerMultiplier),
			TimeSpan.FromMilliseconds(0 * TimerMultiplier));

		ms3.Wait(10000);

		timer1.Dispose();
		timer2.Dispose();
		if (triggerTimes[0] < 30 * TimerMultiplier)
		{
			triggerTimes.RemoveAt(0);
		}

		triggerTimes[0].Should()
			.BeGreaterThan(70 * TimerMultiplier).And
			.BeLessThan(130 * TimerMultiplier);
		for (int i = 1; i < triggerTimes.Count; i++)
		{
			triggerTimes[i].Should()
				.BeGreaterThan(170 * TimerMultiplier).And
				.BeLessThan(230 * TimerMultiplier);
		}
	}

	[SkippableFact]
	public void Dispose_WithManualResetEventWaitHandle_ShouldBeSet()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using ManualResetEvent waitHandle = new(false);
		bool result = timer.Dispose(waitHandle);

		waitHandle.WaitOne(1000).Should().BeTrue();
		result.Should().BeTrue();
		// ReSharper disable once AccessToDisposedClosure
		Exception? exception = Record.Exception(() =>
		{
			timer.Change(0, 0);
		});

#if NET8_0_OR_GREATER
		exception.Should().BeNull();
#else
		exception.Should().BeOfType<ObjectDisposedException>();
#endif
	}

	[SkippableFact]
	public void Dispose_WithMutexWaitHandle_ShouldBeSet()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using Mutex waitHandle = new(false);
		bool result = timer.Dispose(waitHandle);

		waitHandle.WaitOne(1000).Should().BeTrue();
		result.Should().BeTrue();
		// ReSharper disable once AccessToDisposedClosure
		Exception? exception = Record.Exception(() =>
		{
			timer.Change(0, 0);
		});

#if NET8_0_OR_GREATER
		exception.Should().BeNull();
#else
		exception.Should().BeOfType<ObjectDisposedException>();
#endif
	}

	[SkippableFact]
	public void Dispose_WithSemaphoreWaitHandle_ShouldBeSet()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using Semaphore waitHandle = new(1, 1);
		bool result = timer.Dispose(waitHandle);

		waitHandle.WaitOne(1000).Should().BeTrue();
		result.Should().BeTrue();
		// ReSharper disable once AccessToDisposedClosure
		Exception? exception = Record.Exception(() =>
		{
			timer.Change(0, 0);
		});

#if NET8_0_OR_GREATER
		exception.Should().BeNull();
#else
		exception.Should().BeOfType<ObjectDisposedException>();
#endif
	}

	[SkippableFact]
	public void Dispose_WithWaitHandleCalledTwice_ShouldReturnFalse()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using ManualResetEvent waitHandle = new(false);
		timer.Dispose(waitHandle);

		bool result = timer.Dispose(waitHandle);

		result.Should().BeFalse();
	}

#if FEATURE_ASYNC_DISPOSABLE
	[SkippableFact]
	public async Task DisposeAsync_ShouldDisposeTimer()
	{
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		await timer.DisposeAsync();

		// ReSharper disable once AccessToDisposedClosure
		Exception? exception = Record.Exception(() =>
		{
			timer.Change(0, 0);
		});

#if NET8_0_OR_GREATER
		exception.Should().BeNull();
#else
		exception.Should().BeOfType<ObjectDisposedException>();
#endif
	}
#endif
}
