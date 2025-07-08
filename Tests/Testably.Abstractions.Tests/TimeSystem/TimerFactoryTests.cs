using System;
using System.Threading;
using System.Threading.Tasks;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class TimeFactoryTests
{
#if FEATURE_TIMER_COUNT
	[Fact]
	public void ActiveCount_ShouldBeIncrementedWhenCreatingANewTimer()
	{
		using ITimer timer = TimeSystem.Timer.New(_ => { });
		TimeSystem.Timer.ActiveCount.Should().BeGreaterThan(0);
	}
#endif

#if FEATURE_TIMER_COUNT
	[Fact]
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

	[Theory]
	[InlineData(-2)]
	[InlineData(-500)]
	public async Task New_InvalidDueTime_ShouldThrowArgumentOutOfRangeException(int dueTime)
	{
		void Act()
		{
			using ITimer timer = TimeSystem.Timer.New(_ =>
			{
			}, null, dueTime, 0);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086).And.WithParamName(nameof(dueTime));
	}

	[Theory]
	[InlineData(-2)]
	[InlineData(-500)]
	public async Task New_InvalidPeriod_ShouldThrowArgumentOutOfRangeException(int period)
	{
		void Act()
		{
			using ITimer timer = TimeSystem.Timer.New(_ =>
			{
			}, null, 0, period);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086).And.WithParamName(nameof(period));
	}

	[Fact]
	public async Task New_WithPeriod_ShouldStartTimer()
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

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(count).IsGreaterThanOrEqualTo(2);
	}

	[Fact]
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

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await Task.Delay(100, TestContext.Current.CancellationToken);
		await That(count).IsEqualTo(1);
	}

	[Fact]
	public async Task New_WithoutPeriod_ShouldNotStartTimer()
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

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
	}
}
