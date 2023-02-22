using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class TimerTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
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
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086, paramName: "dueTime");
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
			.BeException<ArgumentOutOfRangeException>(hResult: -2146233086, paramName: "period");
	}

	[SkippableFact]
	public void Change_WithInt_ShouldResetTimer()
	{
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
		}, null, 0, 200);
		ms.Wait();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 200);
			ms2.Set();
		}, null, 100, 0);

		ms3.Wait(10000);

		triggerTimes[0].Should().BeLessThan(30);
		triggerTimes[1].Should().BeGreaterThan(70).And.BeLessThan(130);
		for (int i = 2; i < triggerTimes.Count; i++)
		{
			triggerTimes[i].Should().BeGreaterThan(170).And.BeLessThan(230);
		}
	}

	[SkippableFact]
	public void Change_WithLong_ShouldResetTimer()
	{
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
		}, null, 0L, 200L);
		ms.Wait();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0L, 200L);
			ms2.Set();
		}, null, 100L, 0L);

		ms3.Wait(10000);

		triggerTimes[0].Should().BeLessThan(30);
		triggerTimes[1].Should().BeGreaterThan(70).And.BeLessThan(130);
		for (int i = 2; i < triggerTimes.Count; i++)
		{
			triggerTimes[i].Should().BeGreaterThan(170).And.BeLessThan(230);
		}
	}

	[SkippableFact]
	public void Change_WithTimeSpan_ShouldResetTimer()
	{
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
		}, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(200));
		ms.Wait();
		using ITimer timer2 = TimeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(200));
			ms2.Set();
		}, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(0));

		ms3.Wait(10000);

		triggerTimes[0].Should().BeLessThan(30);
		triggerTimes[1].Should().BeGreaterThan(70).And.BeLessThan(130);
		for (int i = 2; i < triggerTimes.Count; i++)
		{
			triggerTimes[i].Should().BeGreaterThan(170).And.BeLessThan(230);
		}
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
}
