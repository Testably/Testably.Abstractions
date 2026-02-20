using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class StopwatchTests
{
	[Fact]
	public async Task Elapsed_ShouldInitializeToZero()
	{
		IStopwatch stopwatch = TimeSystem.Stopwatch.New();
		TimeSpan elapsed = stopwatch.Elapsed;

		await That(elapsed).IsEqualTo(TimeSpan.Zero);
	}

	[Fact]
	public async Task Elapsed_WhenRunning_ShouldIncreaseValue()
	{
		TimeSpan delay = TimeSpan.FromMilliseconds(100);
		IStopwatch stopwatch = TimeSystem.Stopwatch.New();
		stopwatch.Start();

		TimeSpan elapsedBefore = stopwatch.Elapsed;
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);
		TimeSpan elapsedAfter = stopwatch.Elapsed;

		await That(elapsedAfter).IsGreaterThanOrEqualTo(elapsedBefore + delay);
	}

	[Fact]
	public async Task Elapsed_WhenStopped_ShouldRemainUnchanged()
	{
		TimeSpan delay = TimeSpan.FromMilliseconds(100);
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);
		stopwatch.Stop();

		TimeSpan elapsedBefore = stopwatch.Elapsed;
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);
		TimeSpan elapsedAfter = stopwatch.Elapsed;

		await That(elapsedAfter).IsEqualTo(elapsedBefore);
	}

	[Fact]
	public async Task ElapsedMilliseconds_ShouldBeEqualToElapsed()
	{
		TimeSpan delay = TimeSpan.FromMilliseconds(100);
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);

		stopwatch.Stop();
		TimeSpan elapsed = stopwatch.Elapsed;
		long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

		await That(elapsedMilliseconds).IsEqualTo(elapsed.TotalMilliseconds);
	}

	[Fact]
	public async Task ElapsedTicks_ShouldBeEqualToElapsed()
	{
		TimeSpan delay = TimeSpan.FromMilliseconds(100);
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);

		stopwatch.Stop();
		TimeSpan elapsed = stopwatch.Elapsed;
		long elapsedTicks = stopwatch.ElapsedTicks;

		await That(elapsedTicks).IsEqualTo(elapsed.Ticks);
	}

	[Fact]
	public async Task Reset_ShouldResetElapsedAndStop()
	{
		TimeSpan delay = TimeSpan.FromMilliseconds(100);
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);

		stopwatch.Reset();
		TimeSpan elapsed = stopwatch.Elapsed;

		await That(elapsed.TotalMilliseconds).IsEqualTo(0);
		await That(stopwatch.IsRunning).IsFalse();
	}

	[Fact]
	public async Task Restart_ShouldResetElapsedAndSetIsRunningToTrue()
	{
		TimeSpan delay = TimeSpan.FromMilliseconds(100);
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);
		stopwatch.Stop();

		stopwatch.Restart();
		TimeSpan elapsed = stopwatch.Elapsed;

		await That(elapsed.TotalMilliseconds).IsLessThan(100);
		await That(stopwatch.IsRunning).IsTrue();
	}

	[Fact]
	public async Task Start_ShouldSetIsRunningToTrue()
	{
		IStopwatch stopwatch = TimeSystem.Stopwatch.New();

		await That(stopwatch.IsRunning).IsFalse();

		stopwatch.Start();

		await That(stopwatch.IsRunning).IsTrue();
	}

	[Fact]
	public async Task Start_WhenStarted_ShouldDoNothing()
	{
		IStopwatch stopwatch = TimeSystem.Stopwatch.New();
		stopwatch.Start();

		stopwatch.Start();

		await That(stopwatch.IsRunning).IsTrue();
	}

	[Fact]
	public async Task Stop_ShouldSetIsRunningToFalse()
	{
		TimeSpan delay = TimeSpan.FromMilliseconds(100);
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);

		stopwatch.Stop();
		TimeSpan elapsed = stopwatch.Elapsed;

		await That(elapsed.TotalMilliseconds).IsGreaterThanOrEqualTo(100);
		await That(stopwatch.IsRunning).IsFalse();
	}

	[Fact]
	public async Task Stop_WhenStopped_ShouldDoNothing()
	{
		TimeSpan delay = TimeSpan.FromMilliseconds(100);
		IStopwatch stopwatch = TimeSystem.Stopwatch.StartNew();
		await TimeSystem.Task.Delay(delay, TestContext.Current.CancellationToken);

		stopwatch.Stop();
		TimeSpan elapsed1 = stopwatch.Elapsed;
		stopwatch.Stop();
		TimeSpan elapsed2 = stopwatch.Elapsed;

		await That(elapsed2).IsEqualTo(elapsed1);
		await That(stopwatch.IsRunning).IsFalse();
	}
}
