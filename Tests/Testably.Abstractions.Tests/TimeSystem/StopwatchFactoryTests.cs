using System.Diagnostics;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class StopwatchFactoryTests
{
	[Fact]
	public async Task Frequency_ShouldReturnValueOfAtLeastTicksPerSecond()
	{
		var expectedMinimum = TimeSystem is RealTimeSystem
			? Stopwatch.Frequency
			: TimeSpan.TicksPerSecond;
		long frequency = TimeSystem.Stopwatch.Frequency;

		await That(frequency).IsGreaterThanOrEqualTo(expectedMinimum);
	}

#if FEATURE_STOPWATCH_GETELAPSEDTIME
	[Theory]
	[InlineData(5000L, 8000L, 3000L)]
	[InlineData(8000L, 5000L, -3000L)]
	[InlineData(4000L, 4000L, 0L)]
	public async Task GetElapsedTime_WithStartingAndEndingTimestamp_ShouldReturnValue(
		long startingTimestamp, long endingTimestamp, long expectedTicks)
	{
		TimeSpan timestamp =
			TimeSystem.Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);

		long actualStopwatchTicks = timestamp.Ticks * TimeSystem.Stopwatch.Frequency / TimeSpan.TicksPerSecond;
		await That(actualStopwatchTicks).IsEqualTo(expectedTicks);
	}
#endif

#if FEATURE_STOPWATCH_GETELAPSEDTIME
	[Theory]
	[InlineData(1000L)]
	[InlineData(100000L)]
	public async Task GetElapsedTime_WithStartingTimestamp_ShouldReturnValue(long expectedTicks)
	{
		long endingTimestamp = TimeSystem.Stopwatch.GetTimestamp();
		long startingTimestamp = endingTimestamp - expectedTicks;
		TimeSpan timestamp =
			TimeSystem.Stopwatch.GetElapsedTime(startingTimestamp);

		long actualStopwatchTicks = timestamp.Ticks * TimeSystem.Stopwatch.Frequency / TimeSpan.TicksPerSecond;
		await That(actualStopwatchTicks).IsGreaterThanOrEqualTo(expectedTicks);
	}
#endif

	[Fact]
	public async Task GetTimestamp_AfterDelay_ShouldBeDifferent()
	{
		long timestamp1 = TimeSystem.Stopwatch.GetTimestamp();

		await TimeSystem.Task.Delay(10, TestContext.Current.CancellationToken);

		long timestamp2 = TimeSystem.Stopwatch.GetTimestamp();
		await That(timestamp2).IsGreaterThan(timestamp1);
	}

	[Fact]
	public async Task GetTimestamp_ShouldReturnValue()
	{
		long timestamp = TimeSystem.Stopwatch.GetTimestamp();

		await That(timestamp).IsGreaterThan(0);
	}

	[Fact]
	public async Task IsHighResolution_ShouldReturnTrue()
	{
		bool expected = TimeSystem is MockTimeSystem || Stopwatch.IsHighResolution;
		bool isHighResolution = TimeSystem.Stopwatch.IsHighResolution;

		await That(isHighResolution).IsEqualTo(expected);
	}

	[Fact]
	public async Task New_ShouldCreateNotRunningStopwatch()
	{
		IStopwatch stopwatch = TimeSystem.Stopwatch.New();

		await That(stopwatch.IsRunning).IsFalse();
	}

	[Fact]
	public async Task StartNew_ShouldCreateRunningStopwatch()
	{
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();

		await That(stopwatch.IsRunning).IsTrue();
	}
}
