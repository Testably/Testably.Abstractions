using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class TimerFactoryTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
#if FEATURE_TIMER_COUNT
	[SkippableFact]
	public void ActiveCount_ShouldBeIncrementedWhenCreatingANewTimer()
	{
		using ITimer timer = TimeSystem.Timer.New(_ => { });
		TimeSystem.Timer.ActiveCount.Should().BeGreaterThan(0);
	}

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
					timers.Add(TimeSystem.Timer.New(TimerCallback, null, 60000, 60000));
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
			TimeSystem.Timer.New(_ =>
			{
			}, null, dueTime, 0);
		});

		exception.Should()
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086, paramName: "dueTime");
	}

	[SkippableTheory]
	[InlineData(-2)]
	[InlineData(-500)]
	public void New_InvalidPeriod_ShouldThrowArgumentOutOfRangeException(int period)
	{
		Exception? exception = Record.Exception(() =>
		{
			TimeSystem.Timer.New(_ =>
			{
			}, null, 0, period);
		});

		exception.Should()
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086, paramName: "period");
	}

	[SkippableFact]
	public void New_WithPeriod_ShouldStartTimer()
	{
		int count = 0;
		ManualResetEventSlim ms = new ManualResetEventSlim();
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			count++;
			if (count > 1)
			{
				ms.Set();
			}
		}, null, 0, 50);

		ms.Wait(500).Should().BeTrue();
		count.Should().BeGreaterOrEqualTo(2);
	}

	[SkippableFact]
	public void New_WithDueTime_ShouldStartTimerOnce()
	{
		int count = 0;
		ManualResetEventSlim ms = new();
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			count++;
			if (count > 1)
			{
				ms.Set();
			}
		}, null, 50, 0);

		ms.Wait(500).Should().BeFalse();
		count.Should().BeGreaterOrEqualTo(1);
	}

	[SkippableFact]
	public void New_WithoutPeriod_ShouldNotStartTimer()
	{
		int count = 0;
		ManualResetEventSlim ms = new();
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			count++;
			if (count > 1)
			{
				ms.Set();
			}
		});

		ms.Wait(500).Should().BeFalse();
		count.Should().Be(0);
	}
}
