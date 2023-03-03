using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class TimerTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	private const int TimerMultiplier = 10;

	[SkippableFact]
	public void Change_DisposedTimer_ShouldThrowObjectDisposedException()
	{
		ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 0, 200);
		timer.Dispose();

		Exception? exception = Record.Exception(() =>
		{
			timer.Change(100, 200);
		});

		exception.Should().BeOfType<ObjectDisposedException>();
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
		ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
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
		ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			timer.Change(0, period);
		});

		exception.Should()
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086,
				paramName: nameof(period));
	}

	[SkippableFact]
	public void Change_SameValues_ShouldReturnTrue()
	{
		ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		bool result = timer.Change(100, 200);

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
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			DateTime now = TimeSystem.DateTime.Now;
			double diff = (now - previousTime).TotalMilliseconds;
			previousTime = now;
			ms.Set();
			triggerTimes.Add((int)diff);
			ms2.Wait();
			if (triggerTimes.Count > 3)
			{
				ms3.Set();
			}
		}, null, 0 * TimerMultiplier, 200 * TimerMultiplier);
		ms.Wait();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0 * TimerMultiplier, 200 * TimerMultiplier);
			ms2.Set();
		}, null, 100 * TimerMultiplier, 0 * TimerMultiplier);

		ms3.Wait(10000 * TimerMultiplier);

		triggerTimes[0].Should()
			.BeLessThan(30 * TimerMultiplier);
		triggerTimes[1].Should()
			.BeGreaterThan(70 * TimerMultiplier).And
			.BeLessThan(130 * TimerMultiplier);
		for (int i = 2; i < triggerTimes.Count; i++)
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
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			DateTime now = TimeSystem.DateTime.Now;
			double diff = (now - previousTime).TotalMilliseconds;
			previousTime = now;
			ms.Set();
			triggerTimes.Add((int)diff);
			ms2.Wait();
			if (triggerTimes.Count > 3)
			{
				ms3.Set();
			}
		}, null, 0L * TimerMultiplier, 200L * TimerMultiplier);
		ms.Wait();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0L * TimerMultiplier, 200L * TimerMultiplier);
			ms2.Set();
		}, null, 100L * TimerMultiplier, 0L * TimerMultiplier);

		ms3.Wait(10000);

		triggerTimes[0].Should()
			.BeLessThan(30 * TimerMultiplier);
		triggerTimes[1].Should()
			.BeGreaterThan(70 * TimerMultiplier).And
			.BeLessThan(130 * TimerMultiplier);
		for (int i = 2; i < triggerTimes.Count; i++)
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
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			DateTime now = TimeSystem.DateTime.Now;
			double diff = (now - previousTime).TotalMilliseconds;
			previousTime = now;
			ms.Set();
			triggerTimes.Add((int)diff);
			ms2.Wait();
			if (triggerTimes.Count > 3)
			{
				ms3.Set();
			}
		}, null, TimeSpan.FromMilliseconds(0 * TimerMultiplier),
			TimeSpan.FromMilliseconds(200 * TimerMultiplier));
		ms.Wait();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(TimeSpan.FromMilliseconds(0 * TimerMultiplier),
				TimeSpan.FromMilliseconds(200 * TimerMultiplier));
			ms2.Set();
		}, null, TimeSpan.FromMilliseconds(100 * TimerMultiplier),
			TimeSpan.FromMilliseconds(0 * TimerMultiplier));

		ms3.Wait(10000);

		triggerTimes[0].Should()
			.BeLessThan(30 * TimerMultiplier);
		triggerTimes[1].Should()
			.BeGreaterThan(70 * TimerMultiplier).And
			.BeLessThan(130 * TimerMultiplier);
		for (int i = 2; i < triggerTimes.Count; i++)
		{
			triggerTimes[i].Should()
				.BeGreaterThan(170 * TimerMultiplier).And
				.BeLessThan(230 * TimerMultiplier);
		}
	}

	[SkippableFact]
	public void Dispose_WithManualResetEventWaitHandle_ShouldBeSet()
	{
		ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using ManualResetEvent waitHandle = new(false);
		bool result = timer.Dispose(waitHandle);

		waitHandle.WaitOne(1000).Should().BeTrue();
		result.Should().BeTrue();
		Record.Exception(() => timer.Change(0, 0))
			.Should().BeOfType<ObjectDisposedException>();
	}

	[SkippableFact]
	public void Dispose_WithMutexWaitHandle_ShouldBeSet()
	{
		ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using Mutex waitHandle = new(false);
		bool result = timer.Dispose(waitHandle);

		waitHandle.WaitOne(1000).Should().BeTrue();
		result.Should().BeTrue();
		Record.Exception(() => timer.Change(0, 0))
			.Should().BeOfType<ObjectDisposedException>();
	}

	[SkippableFact]
	public void Dispose_WithSemaphoreWaitHandle_ShouldBeSet()
	{
		ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using Semaphore waitHandle = new(1, 1);
		bool result = timer.Dispose(waitHandle);

		waitHandle.WaitOne(1000).Should().BeTrue();
		result.Should().BeTrue();
		Record.Exception(() => timer.Change(0, 0))
			.Should().BeOfType<ObjectDisposedException>();
	}

	[SkippableFact]
	public void Dispose_WithWaitHandleCalledTwice_ShouldReturnFalse()
	{
		ITimer timer = TimeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using ManualResetEvent waitHandle = new(false);
		timer.Dispose(waitHandle);

		bool result = timer.Dispose(waitHandle);

		result.Should().BeFalse();
	}

	/// <summary>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v7.0.0/src/libraries/Common/tests/System/Threading/ThreadTestHelpers.cs#L27" />
	/// </summary>
	private static Thread CreateGuardedThread(out Action waitForThread, Action start)
	{
		Exception? backgroundEx = null;
		Thread t =
			new(() =>
			{
				try
				{
					start();
				}
				catch (Exception ex)
				{
					backgroundEx = ex;
				}
			});

		void LocalCheckForThreadErrors()
		{
			if (backgroundEx != null)
			{
				throw new AggregateException(backgroundEx);
			}
		}

		waitForThread =
			() =>
			{
				Assert.True(t.Join(60000));
				LocalCheckForThreadErrors();
			};
		return t;
	}
}
