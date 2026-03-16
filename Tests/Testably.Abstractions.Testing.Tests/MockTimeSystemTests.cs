using aweXpect.Chronology;
using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.Testing.Tests.TestHelpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests;

public class MockTimeSystemTests
{
	[Test]
	public async Task Delay_DisabledAutoAdvance_ShouldNotChangeTime()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		DateTime before = timeSystem.DateTime.Now;

		await timeSystem.Task.Delay(5.Seconds(), TestContext.Current!.Execution.CancellationToken);

		DateTime after = timeSystem.DateTime.Now;
		await That(after).IsEqualTo(before);
	}

	[Test]
	public async Task Delay_Infinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(Timeout.Infinite,
					TestContext.Current!.Execution.CancellationToken));

		await That(exception).IsNull();
	}

	[Test]
	public async Task Delay_InfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
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
		DateTime now = TimeTestHelper.GetRandomTime(DateTimeKind.Local);

		TimeSpan expectedDifference = TimeZoneInfo.Local.GetUtcOffset(now);

		MockTimeSystem timeSystem = new(DateTime.SpecifyKind(now, dateTimeKind));
		TimeSpan actualDifference = timeSystem.DateTime.Now - timeSystem.DateTime.UtcNow;

		await That(actualDifference).IsEqualTo(expectedDifference);
	}

#if FEATURE_PERIODIC_TIMER
	[Test]
	public async Task PeriodicTimer_DisabledAutoAdvance_ShouldNotAdvanceTime()
	{
		MockTimeSystem timeSystem = new(o => o.DisableAutoAdvance());
		int tickCount = 0;
		using CancellationTokenSource cts =
			CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.Execution
				.CancellationToken);
		CancellationToken token = cts.Token;
		_ = Task.Run(async () =>
		{
			using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
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
	public async Task PeriodicTimer_DisabledAutoAdvance_ShouldTriggerWhenTimeIsManuallyAdvanced(int amount)
	{
		var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(time, o => o.DisableAutoAdvance());
		List<DateTime> tickTimes = [];
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		CancellationToken token = cts.Token;
		_ = Task.Run(async () =>
		{
			try
			{
				using IPeriodicTimer periodicTimer = timeSystem.PeriodicTimer.New(1.Seconds());
				timeSystem.TimeProvider.AdvanceBy(2.Seconds());
				while (await periodicTimer.WaitForNextTickAsync(token))
				{
					tickTimes.Add(timeSystem.DateTime.UtcNow);
				}
			}
			catch (OperationCanceledException)
			{
				// Ignore cancelled tasks
			}
			catch (ObjectDisposedException)
			{
				// Ignore if the semaphore has already been disposed
			}
		}, token);

		for (int i = 1; i < amount; i++)
		{
			await Task.Delay(50);
			timeSystem.TimeProvider.AdvanceBy(2.Seconds());
		}
		await That(tickTimes).HasCount(amount);
		// ReSharper disable once MethodHasAsyncOverload
		cts.Cancel();
	}
#endif

	[Test]
	public async Task Sleep_DisabledAutoAdvance_ShouldNotChangeTime()
	{
		MockTimeSystem timeSystem = new(DateTime.Now, o => o.DisableAutoAdvance());
		DateTime before = timeSystem.DateTime.Now;

		timeSystem.Thread.Sleep(5.Seconds());

		DateTime after = timeSystem.DateTime.Now;
		await That(after).IsEqualTo(before);
	}

	[Test]
	public async Task Sleep_Infinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.Infinite));

		await That(exception).IsNull();
	}

	[Test]
	public async Task Sleep_InfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.InfiniteTimeSpan));

		await That(exception).IsNull();
	}

	[Test]
	public async Task Sleep_LessThanInfinite_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(-2));

		await That(exception).IsExactly<ArgumentOutOfRangeException>();
	}

	[Test]
	public async Task Sleep_LessThanInfiniteTimeSpan_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(()
				=> timeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

		await That(exception).IsExactly<ArgumentOutOfRangeException>();
	}

	[Test]
	public async Task TimeProvider_StartTime_ShouldBeSetToInitialTime()
	{
		DateTime now = TimeTestHelper.GetRandomTime(DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(TimeProvider.Use(now));

		timeSystem.TimeProvider.AdvanceBy(TimeSpan.FromMinutes(42));

		await That(timeSystem.TimeProvider.StartTime).IsEqualTo(now);
	}

	[Test]
	public async Task ToString_WithFixedContainer_ShouldContainTimeProvider()
	{
		DateTime now = TimeTestHelper.GetRandomTime();
		MockTimeSystem timeSystem = new(TimeProvider.Use(now));

		string result = timeSystem.ToString();

		await That(result).Contains("Fixed");
		await That(result).Contains($"{now}Z");
	}

	[Test]
	public async Task ToString_WithNowContainer_ShouldContainTimeProvider()
	{
		MockTimeSystem timeSystem = new(TimeProvider.Now());

		string result = timeSystem.ToString();

		await That(result).Contains("Now");
		await That(result).Contains($"{timeSystem.DateTime.UtcNow}Z");
	}

	[Test]
	public async Task ToString_WithRandomContainer_ShouldContainTimeProvider()
	{
		MockTimeSystem timeSystem = new(TimeProvider.Random());

		string result = timeSystem.ToString();

		await That(result).Contains("Random");
		await That(result).Contains($"{timeSystem.DateTime.UtcNow}Z");
	}
}
