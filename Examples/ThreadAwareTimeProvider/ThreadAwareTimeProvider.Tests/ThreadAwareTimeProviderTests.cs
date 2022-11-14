using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Testably.Abstractions.Testing;
using Xunit;

namespace ThreadAwareTimeProvider.Tests;

public class ThreadAwareTimeProviderTests
{
	[Fact]
	public async Task ParallelTasks_ShouldHaveDistinctTimes()
	{
		var parallelTasks = 10;
		var stepsPerTask = 20;
		ThreadAwareTimeProvider timeProvider = new();
		MockTimeSystem timeSystem = new(timeProvider);
		var start = timeSystem.DateTime.UtcNow;
		ConcurrentDictionary<int, List<int>> delaysPerTask = new();

		for (var i = 1; i <= parallelTasks; i++)
		{
			var taskId = i;
			var taskDelay = TimeSpan.FromSeconds(taskId);
			await Task.Run(async () =>
			{
				for (var j = 0; j < stepsPerTask; j++)
				{
					await timeSystem.Task.Delay(taskDelay);
					var diff = (int)(timeSystem.DateTime.UtcNow - start)
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

		for (var i = 1; i <= parallelTasks; i++)
		{
			var delayPerTask = i * 1000;
			delaysPerTask[i]
				.Should()
				.BeEquivalentTo(
					Enumerable.Range(1, stepsPerTask).Select(x => x * delayPerTask));
		}
	}

	[Fact]
	public void ParallelThreads_ShouldHaveDistinctTimes()
	{
		var parallelThreads = 10;
		var stepsPerThread = 20;
		ThreadAwareTimeProvider timeProvider = new();
		MockTimeSystem timeSystem = new(timeProvider);
		var start = timeSystem.DateTime.UtcNow;
		ConcurrentDictionary<int, List<int>> delaysPerThread = new();
		CountdownEvent ms = new(parallelThreads);

		for (var i = 1; i <= parallelThreads; i++)
		{
			var threadId = i;
			var threadDelay = TimeSpan.FromSeconds(threadId);
			new Thread(() =>
			{
				for (var j = 0; j < stepsPerThread; j++)
				{
					timeSystem.Thread.Sleep(threadDelay);
					var diff = (int)(timeSystem.DateTime.UtcNow - start)
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
		for (var i = 1; i <= parallelThreads; i++)
		{
			var delayPerThread = i * 1000;
			delaysPerThread[i]
				.Should()
				.BeEquivalentTo(
					Enumerable.Range(1, stepsPerThread).Select(x => x * delayPerThread));
		}
	}

	[Theory]
	[InlineData(true, 3000)]
	[InlineData(false, 2000)]
	public async Task SynchronizeClock_AdvanceBy_ShouldUseSynchronizedValueAsBase(
		bool synchronizeClock, int expectedDelay)
	{
		ThreadAwareTimeProvider timeProvider = new();
		MockTimeSystem timeSystem = new(timeProvider);
		var start = timeSystem.DateTime.UtcNow;
		await timeSystem.Task.Delay(1000);

		ManualResetEventSlim ms = new();
		await Task.Run(async () =>
		{
			await timeSystem.Task.Delay(1000);
			if (synchronizeClock)
			{
				timeProvider.SynchronizeClock();
			}

			ms.Set();
		});
		ms.Wait();

		timeSystem.TimeProvider.AdvanceBy(TimeSpan.FromMilliseconds(1000));
		var mainThreadAfterCompletedTaskDelay =
			(int)(timeSystem.DateTime.UtcNow - start).TotalMilliseconds;
		mainThreadAfterCompletedTaskDelay.Should().Be(expectedDelay);
	}

	[Theory]
	[InlineData(true, 2000)]
	[InlineData(false, 1000)]
	public async Task SynchronizeClock_ShouldSetNowToValueOfCurrentAsyncContext(
		bool synchronizeClock, int expectedDelay)
	{
		ThreadAwareTimeProvider timeProvider = new();
		MockTimeSystem timeSystem = new(timeProvider);
		var start = timeSystem.DateTime.UtcNow;
		await timeSystem.Task.Delay(1000);
		var parallelTaskDelay = 0;

		ManualResetEventSlim ms = new();
		await Task.Run(async () =>
		{
			await timeSystem.Task.Delay(1000);
			parallelTaskDelay =
				(int)(timeSystem.DateTime.UtcNow - start).TotalMilliseconds;
			if (synchronizeClock)
			{
				timeProvider.SynchronizeClock();
			}

			ms.Set();
		});
		ms.Wait();

		var mainThreadAfterCompletedTaskDelay =
			(int)(timeSystem.DateTime.UtcNow - start).TotalMilliseconds;
		parallelTaskDelay.Should().Be(2000);
		mainThreadAfterCompletedTaskDelay.Should().Be(expectedDelay);
	}
}