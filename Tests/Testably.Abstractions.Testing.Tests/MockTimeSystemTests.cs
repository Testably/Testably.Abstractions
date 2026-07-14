using System.Threading;
using aweXpect.Chronology;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#if FEATURE_PERIODIC_TIMER
using System.Collections.Generic;
#endif

namespace Testably.Abstractions.Testing.Tests;

public class MockTimeSystemTests
{
	[Test]
	public async Task
		Delay_DisabledAutoAdvance_InfiniteTimeSpan_ShouldOnlyBeReleasedByCancellation()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		using var cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);

		var delayTask = timeSystem.Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);

		// Advancing the clock never releases an infinite delay.
		timeSystem.TimeProvider.AdvanceBy(TimeSpan.FromDays(1));
		await That(delayTask.IsCompleted).IsFalse();

		cts.Cancel();

		async Task Act() => await delayTask;
		await That(Act).Throws<OperationCanceledException>();
	}

	[Test]
	public async Task
		Delay_DisabledAutoAdvance_MultiplePendingDelays_ShouldEachBeReleasedAtTheirDueTime()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		var token = TestContext.Current!.Execution.CancellationToken;

		var delay1 = timeSystem.Task.Delay(1.Seconds(), token);
		var delay2 = timeSystem.Task.Delay(2.Seconds(), token);
		var delay3 = timeSystem.Task.Delay(3.Seconds(), token);

		timeSystem.TimeProvider.AdvanceBy(2.Seconds());

		await delay1;
		await delay2;
		await That(delay1.IsCompleted).IsTrue();
		await That(delay2.IsCompleted).IsTrue();
		await That(delay3.IsCompleted).IsFalse();

		timeSystem.TimeProvider.AdvanceBy(1.Seconds());

		await delay3;
		await That(delay3.IsCompleted).IsTrue();
	}

	[Test]
	public async Task Delay_DisabledAutoAdvance_PartialAdvance_ShouldNotReleaseDelay()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		var token = TestContext.Current!.Execution.CancellationToken;

		var delayTask = timeSystem.Task.Delay(5.Seconds(), token);

		timeSystem.TimeProvider.AdvanceBy(3.Seconds());
		await That(delayTask.IsCompleted).IsFalse();

		timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		await delayTask;
		await That(delayTask.IsCompleted).IsTrue();
	}

	[Test]
	public async Task Delay_DisabledAutoAdvance_SetTo_ShouldNotReleaseDelay_OnlyAdvanceByDoes()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		var token = TestContext.Current!.Execution.CancellationToken;

		var delayTask = timeSystem.Task.Delay(5.Seconds(), token);

		// Jumping the wall clock (e.g. an NTP/DST change) must NOT release a monotonic delay.
		timeSystem.TimeProvider.SetTo(timeSystem.DateTime.UtcNow.AddHours(1));
		await That(delayTask.IsCompleted).IsFalse();

		// Only elapsed (monotonic) time releases it.
		timeSystem.TimeProvider.AdvanceBy(5.Seconds());
		await delayTask;
		await That(delayTask.IsCompleted).IsTrue();
	}

	[Test]
	public async Task Delay_DisabledAutoAdvance_ShouldStayPendingUntilTimeIsAdvanced()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		var token = TestContext.Current!.Execution.CancellationToken;
		var before = timeSystem.DateTime.Now;

		var delayTask = timeSystem.Task.Delay(5.Seconds(), token);

		// The delay stays pending and does not advance time on its own.
		await That(delayTask.IsCompleted).IsFalse();
		await That(timeSystem.DateTime.Now).IsEqualTo(before);

		timeSystem.TimeProvider.AdvanceBy(5.Seconds());

		await delayTask;
		await That(delayTask.IsCompleted).IsTrue();
	}

	[Test]
	public async Task Delay_DisabledAutoAdvance_Zero_ShouldCompleteImmediately()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		var token = TestContext.Current!.Execution.CancellationToken;

		var delayTask = timeSystem.Task.Delay(TimeSpan.Zero, token);

		await delayTask;
		await That(delayTask.IsCompleted).IsTrue();
	}

	[Test]
	public async Task Delay_Infinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		var exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(Timeout.Infinite,
					TestContext.Current!.Execution.CancellationToken));

		await That(exception).IsNull();
	}

	[Test]
	public async Task Delay_InfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		var exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(Timeout.InfiniteTimeSpan,
					TestContext.Current!.Execution.CancellationToken));

		await That(exception).IsNull();
	}

	[Test]
	public async Task Delay_LessThanInfinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();

		async Task Act()
			=> await timeSystem.Task.Delay(-2, TestContext.Current!.Execution.CancellationToken);

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>()
			.WithParamName("millisecondsDelay");
	}

	[Test]
	public async Task Delay_LessThanInfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();

		async Task Act()
			=> await timeSystem.Task.Delay(TimeSpan.FromMilliseconds(-2),
				TestContext.Current!.Execution.CancellationToken);

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("delay");
	}

	[Test]
	[Arguments(DateTimeKind.Local)]
	[Arguments(DateTimeKind.Unspecified)]
	[Arguments(DateTimeKind.Utc)]
	public async Task
		DifferenceBetweenDateTimeNowAndDateTimeUtcNow_ShouldBeLocalTimeZoneOffsetFromUtc(
			DateTimeKind dateTimeKind)
	{
		var now = TimeTestHelper.GetRandomTime(DateTimeKind.Local);

		var expectedDifference = TimeZoneInfo.Local.GetUtcOffset(now);

		MockTimeSystem timeSystem = new(DateTime.SpecifyKind(now, dateTimeKind));
		var actualDifference = timeSystem.DateTime.Now - timeSystem.DateTime.UtcNow;

		await That(actualDifference).IsEqualTo(expectedDifference);
	}

	[Test]
	public async Task DefaultConstructor_ShouldUseLocalTimeZone()
	{
		MockTimeSystem timeSystem = new();

		await That(timeSystem.TimeZoneInfo.Local).IsEqualTo(TimeZoneInfo.Local);
	}

	[Test]
	public async Task DefaultConstructor_NowToUniversalTime_ShouldEqualUtcNow()
	{
		MockTimeSystem timeSystem = new();

		var now = timeSystem.DateTime.Now;

		await That(now.ToUniversalTime()).IsEqualTo(timeSystem.DateTime.UtcNow);
	}

#if FEATURE_PERIODIC_TIMER
	[Test]
	public async Task PeriodicTimer_DisabledAutoAdvance_ShouldNotAdvanceTime()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		int tickCount = 0;
		using var cts =
			CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.Execution
				.CancellationToken);
		var token = cts.Token;
		_ = Task.Run(async () =>
		{
			using var periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
			while (await periodicTimer.WaitForNextTickAsync(token))
			{
				tickCount++;
			}
		}, token);

		await Task.Delay(50.Milliseconds(), token);
		await That(tickCount).IsEqualTo(0);
		cts.Cancel();
	}
#endif

#if FEATURE_PERIODIC_TIMER
	[Test]
	[Arguments(5)]
	public async Task PeriodicTimer_DisabledAutoAdvance_ShouldTriggerWhenTimeIsManuallyAdvanced(
		int amount)
	{
		DateTime time = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(time, o => o.DisableAutoAdvance());
		List<DateTime> tickTimes = [];
		using var cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		cts.CancelAfter(30.Seconds());
		var token = cts.Token;
		using SemaphoreSlim waitStarted = new(0, amount);
		using SemaphoreSlim tickObserved = new(0, amount);

		var backgroundTask = Task.Run(async () =>
		{
			using var periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
			for (int i = 0; i < amount; i++)
			{
				var waitForTickTask = periodicTimer.WaitForNextTickAsync(token);
				// ReSharper disable once AccessToDisposedClosure
				waitStarted.Release();
				_ = await waitForTickTask;
				tickTimes.Add(timeSystem.DateTime.UtcNow);
				// ReSharper disable once AccessToDisposedClosure
				tickObserved.Release();
			}
		}, token);

		for (int i = 0; i < amount; i++)
		{
			await waitStarted.WaitAsync(token);
			timeSystem.TimeProvider.AdvanceBy(2.Seconds());
			await tickObserved.WaitAsync(token);
		}

		await backgroundTask;
		await That(tickTimes).HasCount(amount);
		await That(tickTimes).All().Satisfy(t => t.Second % 2 == 0);
	}
#endif

	[Test]
	[Arguments(DateTimeKind.Utc)]
	[Arguments(DateTimeKind.Local)]
	[Arguments(DateTimeKind.Unspecified)]
	public async Task SetTo_ShouldNormalizeToUtc_ConsistentlyWithConstructor(DateTimeKind kind)
	{
		var time = DateTime.SpecifyKind(new DateTime(2026, 1, 15, 10, 0, 0), kind);
		MockTimeSystem fromConstructor = new(time);
		MockTimeSystem fromSetTo = new(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));

		fromSetTo.TimeProvider.SetTo(time);

		await That(fromSetTo.DateTime.UtcNow).IsEqualTo(fromConstructor.DateTime.UtcNow);
		await That(fromSetTo.DateTime.UtcNow.Kind).IsEqualTo(DateTimeKind.Utc);
		await That(fromSetTo.DateTime.Now).IsEqualTo(fromConstructor.DateTime.Now);
	}

	[Test]
	public async Task SetTo_UnspecifiedKind_ShouldBeTreatedAsUtc()
	{
		DateTime unspecified = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
		MockTimeSystem timeSystem = new(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));

		timeSystem.TimeProvider.SetTo(unspecified);

		await That(timeSystem.DateTime.UtcNow)
			.IsEqualTo(new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc));
	}

	[Test]
	public async Task Sleep_DisabledAutoAdvance_ShouldNotChangeTime()
	{
		MockTimeSystem timeSystem = new(DateTime.Now, o => o.DisableAutoAdvance());
		var before = timeSystem.DateTime.Now;

		timeSystem.Thread.Sleep(5.Seconds());

		var after = timeSystem.DateTime.Now;
		await That(after).IsEqualTo(before);
	}

	[Test]
	public async Task Sleep_Infinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		var exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.Infinite));

		await That(exception).IsNull();
	}

	[Test]
	public async Task Sleep_InfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		var exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.InfiniteTimeSpan));

		await That(exception).IsNull();
	}

	[Test]
	public async Task Sleep_LessThanInfinite_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		var exception =
			Record.Exception(() => timeSystem.Thread.Sleep(-2));

		await That(exception).IsExactly<ArgumentOutOfRangeException>();
	}

	[Test]
	public async Task Sleep_LessThanInfiniteTimeSpan_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		var exception =
			Record.Exception(()
				=> timeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

		await That(exception).IsExactly<ArgumentOutOfRangeException>();
	}

	[Test]
	public async Task TimeProvider_StartTime_ShouldBeSetToInitialTime()
	{
		var now = TimeTestHelper.GetRandomTime(DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(TimeProviderFactory.Use(now));

		timeSystem.TimeProvider.AdvanceBy(TimeSpan.FromMinutes(42));

		await That(timeSystem.TimeProvider.StartTime).IsEqualTo(now);
	}

	[Test]
	public async Task ToString_WithFixedContainer_ShouldContainTimeProvider()
	{
		var now = TimeTestHelper.GetRandomTime();
		MockTimeSystem timeSystem = new(TimeProviderFactory.Use(now));

		string result = timeSystem.ToString();

		await That(result).Contains("Fixed");
		await That(result).Contains($"{now}Z");
	}

	[Test]
	public async Task ToString_WithNowContainer_ShouldContainTimeProvider()
	{
		MockTimeSystem timeSystem = new(TimeProviderFactory.Now());

		string result = timeSystem.ToString();

		await That(result).Contains("Now");
		await That(result).Contains($"{timeSystem.DateTime.UtcNow}Z");
	}

	[Test]
	public async Task ToString_WithRandomContainer_ShouldContainTimeProvider()
	{
		MockTimeSystem timeSystem = new(TimeProviderFactory.Random());

		string result = timeSystem.ToString();

		await That(result).Contains("Random");
		await That(result).Contains($"{timeSystem.DateTime.UtcNow}Z");
	}
}
