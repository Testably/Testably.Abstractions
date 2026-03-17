using aweXpect.Chronology;
using System.Diagnostics;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public class StopwatchFactoryTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task Frequency_ShouldReturnValueOfAtLeastTicksPerSecond()
	{
		var expectedMinimum = TimeSystem is RealTimeSystem
			? Stopwatch.Frequency
			: TimeSpan.TicksPerSecond;
		long frequency = TimeSystem.Stopwatch.Frequency;

		await That(frequency).IsGreaterThanOrEqualTo(expectedMinimum);
	}

#if FEATURE_STOPWATCH_GETELAPSEDTIME
	[Test]
	[Arguments(5000L, 8000L, 3000L)]
	[Arguments(8000L, 5000L, -3000L)]
	[Arguments(4000L, 4000L, 0L)]
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
	[Test]
	[Arguments(1000L)]
	[Arguments(100000L)]
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

	[Test]
	public async Task GetTimestamp_AfterDelay_ShouldBeDifferent()
	{
		long timestamp1 = TimeSystem.Stopwatch.GetTimestamp();

		await TimeSystem.Task.Delay(10, CancellationToken);

		long timestamp2 = TimeSystem.Stopwatch.GetTimestamp();
		await That(timestamp2).IsGreaterThan(timestamp1);
	}

	[Test]
	public async Task GetTimestamp_DividedByFrequency_ShouldReturnSeconds()
	{
		long timestamp1 = TimeSystem.Stopwatch.GetTimestamp();

		await TimeSystem.Task.Delay(0.4.Seconds(), CancellationToken);

		long timestamp2 = TimeSystem.Stopwatch.GetTimestamp();
		await That((double)(timestamp2 - timestamp1) / TimeSystem.Stopwatch.Frequency)
			.IsEqualTo(0.4).Within(0.1);
	}

	[Test]
	public async Task GetTimestamp_ShouldReturnValue()
	{
		long timestamp = TimeSystem.Stopwatch.GetTimestamp();

		await That(timestamp).IsGreaterThan(0);
	}

	[Test]
	public async Task IsHighResolution_ShouldReturnTrue()
	{
		bool expected = TimeSystem is MockTimeSystem || Stopwatch.IsHighResolution;
		bool isHighResolution = TimeSystem.Stopwatch.IsHighResolution;

		await That(isHighResolution).IsEqualTo(expected);
	}

	[Test]
	public async Task New_ShouldCreateNotRunningStopwatch()
	{
		IStopwatch stopwatch = TimeSystem.Stopwatch.New();

		await That(stopwatch.IsRunning).IsFalse();
	}

	[Test]
	public async Task StartNew_ShouldCreateRunningStopwatch()
	{
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();

		await That(stopwatch.IsRunning).IsTrue();
	}
}
