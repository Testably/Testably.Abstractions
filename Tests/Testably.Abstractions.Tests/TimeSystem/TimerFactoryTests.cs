using System.Threading;
using System.Threading.Tasks;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class TimeFactoryTests
{
#if FEATURE_TIMER_COUNT
	[SkippableFact]
	public void ActiveCount_ShouldBeIncrementedWhenCreatingANewTimer()
	{
		using ITimer timer = TimeSystem.Timer.New(_ => { });
		TimeSystem.Timer.ActiveCount.Should().BeGreaterThan(0);
	}
#endif

#if FEATURE_TIMER_COUNT
	[SkippableFact]
	public void ActiveCount_ShouldBeResetWhenDisposingATimer()
	{
		const int timersPerThread = 64;
		int processorCount = Environment.ProcessorCount;
		int totalTimerCount = processorCount * timersPerThread;

		List<ITimer>? timers = new(totalTimerCount);

		void TimerCallback(object? _)
		{
		}

		ManualResetEvent? startCreateTimerThreads = new(false);

		void CreateTimerThreadStart()
		{
			startCreateTimerThreads.WaitOne();
			for (int i = 0; i < timersPerThread; ++i)
			{
				lock (timers)
				{
					timers.Add(TimeSystem.Timer.New(TimerCallback, null, ExpectSuccess, ExpectSuccess));
					Assert.True(TimeSystem.Timer.ActiveCount >= timers.Count);
				}
			}
		}

		Action[] waitsForThread = new Action[processorCount];
		for (int i = 0; i < processorCount; ++i)
		{
			Thread t =
				ThreadTestHelpers.CreateGuardedThread(out waitsForThread[i],
					CreateTimerThreadStart);
			t.IsBackground = true;
			t.Start();
		}

		startCreateTimerThreads.Set();
		foreach (Action waitForThread in waitsForThread)
		{
			waitForThread();
		}

		while (timers.Count > 0)
		{
			long timerCountBeforeRemove = TimeSystem.Timer.ActiveCount;
			int endIndex = timers.Count - (processorCount * 8);
			for (int i = timers.Count - 1; i >= Math.Max(0, endIndex); --i)
			{
				timers[i].Dispose();
				timers.RemoveAt(i);
			}

			if (endIndex >= 0)
			{
				Assert.True(TimeSystem.Timer.ActiveCount < timerCountBeforeRemove);
			}
		}
	}
#endif

	[SkippableTheory]
	[InlineData(-2)]
	[InlineData(-500)]
	public void New_InvalidDueTime_ShouldThrowArgumentOutOfRangeException(int dueTime)
	{
		Exception? exception = Record.Exception(() =>
		{
			using ITimer timer = TimeSystem.Timer.New(_ =>
			{
			}, null, dueTime, 0);
		});

		exception.Should()
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086,
				paramName: nameof(dueTime));
	}

	[SkippableTheory]
	[InlineData(-2)]
	[InlineData(-500)]
	public void New_InvalidPeriod_ShouldThrowArgumentOutOfRangeException(int period)
	{
		Exception? exception = Record.Exception(() =>
		{
			using ITimer timer = TimeSystem.Timer.New(_ =>
			{
			}, null, 0, period);
		});

		exception.Should()
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086,
				paramName: nameof(period));
	}

	[SkippableFact]
	public void New_WithPeriod_ShouldStartTimer()
	{
		int count = 0;
		using ManualResetEventSlim ms = new();
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				count++;
				if (count > 1)
				{
					ms.Set();
				}
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}, null, 0, 50);

		ms.Wait(ExpectSuccess).Should().BeTrue();
		count.Should().BeGreaterOrEqualTo(2);
	}

	[SkippableFact]
	public async Task New_WithDueTime_ShouldStartTimerOnce()
	{
		int count = 0;
		using ManualResetEventSlim ms = new();
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				count++;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}, null, 5, 0);

		ms.Wait(ExpectSuccess).Should().BeTrue();
		await Task.Delay(100);
		count.Should().Be(1);
	}

	[SkippableFact]
	public void New_WithoutPeriod_ShouldNotStartTimer()
	{
		using ManualResetEventSlim ms = new();
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		});

		ms.Wait(EnsureTimeout).Should().BeFalse();
	}
}
