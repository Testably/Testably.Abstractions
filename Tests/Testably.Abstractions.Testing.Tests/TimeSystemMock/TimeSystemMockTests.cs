using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.Tests.TimeSystemMock;

public class TimeSystemMockTests
{
	[Fact]
	public async Task Delay_Infinite_ShouldNotThrowException()
	{
		Testing.TimeSystemMock timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(() => timeSystem.Task.Delay(Timeout.Infinite));

		exception.Should().BeNull();
	}

	[Fact]
	public async Task Delay_InfiniteTimeSpan_ShouldNotThrowException()
	{
		Testing.TimeSystemMock timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(Timeout.InfiniteTimeSpan));

		exception.Should().BeNull();
	}

	[Fact]
	public async Task Delay_LessThanInfinite_ShouldNotThrowException()
	{
		Testing.TimeSystemMock timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(() => timeSystem.Task.Delay(-2));

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
		   .Which.ParamName.Should().Be("millisecondsDelay");
	}

	[Fact]
	public async Task Delay_LessThanInfiniteTimeSpan_ShouldNotThrowException()
	{
		Testing.TimeSystemMock timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(TimeSpan.FromMilliseconds(-2)));

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
		   .Which.ParamName.Should().Be("delay");
	}

	[Fact]
	public async Task ParallelTasks_ShouldHaveDistinctTimes()
	{
		int parallelTasks = 10;
		int stepsPerTask = 20;
		Testing.TimeSystemMock timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		ConcurrentDictionary<int, List<int>> delaysPerTask = new();

		for (int i = 1; i <= parallelTasks; i++)
		{
			int taskId = i;
			TimeSpan taskDelay = TimeSpan.FromSeconds(taskId);
			await Task.Run(async () =>
			{
				for (int j = 0; j < stepsPerTask; j++)
				{
					await timeSystem.Task.Delay(taskDelay);
					int diff = (int)(timeSystem.DateTime.UtcNow - start)
					   .TotalMilliseconds;
					delaysPerTask.AddOrUpdate(taskId,
						_ => new List<int> { diff },
						(_, l) =>
						{
							l.Add(diff);
							return l;
						});
				}
			});
		}

		for (int i = 1; i <= parallelTasks; i++)
		{
			int delayPerTask = i * 1000;
			delaysPerTask[i].Should().BeEquivalentTo(
				Enumerable.Range(1, stepsPerTask).Select(x => x * delayPerTask));
		}
	}

	[Theory]
	[InlineData(true, 3000)]
	[InlineData(false, 2000)]
	public async Task SynchronizeClock_AdvanceBy_ShouldUseSynchronizedValueAsBase(
		bool synchronizeClock, int expectedDelay)
	{
		Testing.TimeSystemMock timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		await timeSystem.Task.Delay(1000);

		ManualResetEventSlim ms = new();
		await Task.Run(async () =>
		{
			await timeSystem.Task.Delay(1000);
			if (synchronizeClock)
			{
				timeSystem.TimeProvider.SynchronizeClock();
			}

			ms.Set();
		});
		ms.Wait();

		timeSystem.TimeProvider.AdvanceBy(TimeSpan.FromMilliseconds(1000));
		int mainThreadAfterCompletedTaskDelay =
			(int)(timeSystem.DateTime.UtcNow - start).TotalMilliseconds;
		mainThreadAfterCompletedTaskDelay.Should().Be(expectedDelay);
	}

	[Theory]
	[InlineData(true, 2000)]
	[InlineData(false, 1000)]
	public async Task SynchronizeClock_ShouldSetNowToValueOfCurrentAsyncContext(
		bool synchronizeClock, int expectedDelay)
	{
		Testing.TimeSystemMock timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		await timeSystem.Task.Delay(1000);
		int parallelTaskDelay = 0;

		ManualResetEventSlim ms = new();
		await Task.Run(async () =>
		{
			await timeSystem.Task.Delay(1000);
			parallelTaskDelay =
				(int)(timeSystem.DateTime.UtcNow - start).TotalMilliseconds;
			if (synchronizeClock)
			{
				timeSystem.TimeProvider.SynchronizeClock();
			}

			ms.Set();
		});
		ms.Wait();

		int mainThreadAfterCompletedTaskDelay =
			(int)(timeSystem.DateTime.UtcNow - start).TotalMilliseconds;
		parallelTaskDelay.Should().Be(2000);
		mainThreadAfterCompletedTaskDelay.Should().Be(expectedDelay);
	}

	[Fact]
	public void ParallelThreads_ShouldHaveDistinctTimes()
	{
		int parallelThreads = 10;
		int stepsPerThread = 20;
		Testing.TimeSystemMock timeSystem = new();
		DateTime start = timeSystem.DateTime.UtcNow;
		ConcurrentDictionary<int, List<int>> delaysPerThread = new();
		CountdownEvent ms = new(parallelThreads);

		for (int i = 1; i <= parallelThreads; i++)
		{
			int threadId = i;
			TimeSpan threadDelay = TimeSpan.FromSeconds(threadId);
			new Thread(() =>
			{
				for (int j = 0; j < stepsPerThread; j++)
				{
					timeSystem.Thread.Sleep(threadDelay);
					int diff = (int)(timeSystem.DateTime.UtcNow - start)
					   .TotalMilliseconds;
					delaysPerThread.AddOrUpdate(threadId,
						_ => new List<int> { diff },
						(_, l) =>
						{
							l.Add(diff);
							return l;
						});
				}

				ms.Signal();
			}).Start();
		}

		ms.Wait(1000).Should().BeTrue();
		for (int i = 1; i <= parallelThreads; i++)
		{
			int delayPerThread = i * 1000;
			delaysPerThread[i].Should().BeEquivalentTo(
				Enumerable.Range(1, stepsPerThread).Select(x => x * delayPerThread));
		}
	}

	[Fact]
	public void Sleep_Infinite_ShouldNotThrowException()
	{
		Testing.TimeSystemMock timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.Infinite));

		exception.Should().BeNull();
	}

	[Fact]
	public void Sleep_InfiniteTimeSpan_ShouldNotThrowException()
	{
		Testing.TimeSystemMock timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.InfiniteTimeSpan));

		exception.Should().BeNull();
	}

	[Fact]
	public void Sleep_LessThanInfinite_ShouldNotThrowException()
	{
		Testing.TimeSystemMock timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(-2));

		exception.Should().BeOfType<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Sleep_LessThanInfiniteTimeSpan_ShouldNotThrowException()
	{
		Testing.TimeSystemMock timeSystem = new();
		Exception? exception =
			Record.Exception(()
				=> timeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

		exception.Should().BeOfType<ArgumentOutOfRangeException>();
	}
}