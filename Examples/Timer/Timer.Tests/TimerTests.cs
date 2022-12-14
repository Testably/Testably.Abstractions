using FluentAssertions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.Timer.Tests;

public class TimerTests
{
	[Fact]
	public void Callback_TakesEqualToInterval_ShouldNotDelay()
	{
		TimeSpan interval = TimeSpan.FromSeconds(10);
		MockTimeSystem timeSystem = new();
		List<TimeSpan> delays = new();
		timeSystem.On.TaskDelay(delay =>
		{
			delays.Add(delay);
		});

		ManualResetEventSlim ms = new();
		int expectedIterations = 10;

		Timer timer = timeSystem.CreateTimer(
			interval,
			_ =>
			{
				timeSystem.Thread.Sleep(interval);
				expectedIterations--;
				if (expectedIterations < 0)
				{
					ms.Set();
				}
			});
		timer.Start();

		ms.Wait(1000);
		timer.Dispose();

		delays.Should().BeEmpty();
	}

	[Fact]
	public void Callback_TakesLongerThanInterval_ShouldNotDelay()
	{
		TimeSpan interval = TimeSpan.FromSeconds(10);
		MockTimeSystem timeSystem = new();
		List<TimeSpan> delays = new();
		timeSystem.On.TaskDelay(delay =>
		{
			delays.Add(delay);
		});

		ManualResetEventSlim ms = new();
		int expectedIterations = 10;

		Timer timer = timeSystem.CreateTimer(
			interval,
			_ =>
			{
				// delay of -1 milliseconds is treated as infinity, but should not be applied!
				timeSystem.Thread.Sleep(interval + TimeSpan.FromMilliseconds(1));
				expectedIterations--;
				if (expectedIterations < 0)
				{
					ms.Set();
				}
			});
		timer.Start();

		ms.Wait(1000);
		timer.Dispose();

		delays.Should().BeEmpty();
	}

	[Fact]
	public void CreateTimer_Dispose_ShouldStopIterations()
	{
		MockTimeSystem timeSystem = Initialize(
			out TimeSpan interval,
			out int expectedIterations,
			out TimeSpan[] receivedIntervals);
		timeSystem.On.TaskDelay(d =>
			receivedIntervals[Math.Max(0, expectedIterations)] = d);
		Timer? runningTimer = null;
		ManualResetEventSlim ms = new();

		Timer timer = timeSystem.CreateTimer(interval, _ =>
		{
			expectedIterations--;
			if (expectedIterations < 0)
			{
				// ReSharper disable once AccessToModifiedClosure
				runningTimer?.Dispose();
				ms.Set();
			}
		});
		runningTimer = timer.Start();
		ms.Wait(1000);

		receivedIntervals.Should().AllBeEquivalentTo(interval);
	}

	[Fact]
	public void CreateTimer_Exception_ShouldCallOnErrorAndContinueIterations()
	{
		Exception exception = new("foo");
		List<Exception> receivedExceptions = new();
		MockTimeSystem timeSystem = Initialize(
			out TimeSpan interval,
			out int expectedIterations,
			out TimeSpan[] receivedIntervals);
		timeSystem.On.TaskDelay(d =>
			receivedIntervals[Math.Max(0, expectedIterations)] = d);
		Timer? runningTimer = null;
		ManualResetEventSlim ms = new();

		Timer timer = timeSystem.CreateTimer(interval, _ =>
		{
			expectedIterations--;
			if (expectedIterations % 3 == 0)
			{
				throw exception;
			}

			if (expectedIterations < 0)
			{
				// ReSharper disable once AccessToModifiedClosure
				runningTimer?.Dispose();
				ms.Set();
			}
		}, e => receivedExceptions.Add(e));
		runningTimer = timer.Start();
		ms.Wait(1000);

		receivedIntervals.Should().AllBeEquivalentTo(interval);
		receivedExceptions.Should().AllBeEquivalentTo(exception);
		receivedExceptions.Count.Should()
			.BeGreaterOrEqualTo(receivedIntervals.Length / 3);
	}

	[Fact]
	public void CreateTimer_InfiniteDelay_ShouldUseTimeSpanZero()
	{
		MockTimeSystem timeSystem = Initialize(
			out _,
			out int expectedIterations,
			out TimeSpan[] receivedIntervals);
		timeSystem.On.TaskDelay(d =>
			receivedIntervals[Math.Max(0, expectedIterations)] = d);
		CancellationTokenSource cancellationTokenSource = new();
		ManualResetEventSlim ms = new();

		Timer timer = timeSystem.CreateTimer(Timeout.InfiniteTimeSpan, _ =>
		{
			expectedIterations--;
			if (expectedIterations < 0)
			{
				cancellationTokenSource.Cancel();
				ms.Set();
			}
		});
		timer.Start(cancellationTokenSource.Token);
		ms.Wait(1000);

		receivedIntervals.Should().AllBeEquivalentTo(TimeSpan.Zero);
	}

	[Fact]
	public void CreateTimer_Start_ShouldIterateUntilCancelled()
	{
		MockTimeSystem timeSystem = Initialize(
			out TimeSpan interval,
			out int expectedIterations,
			out TimeSpan[] receivedIntervals);
		timeSystem.On.TaskDelay(d =>
			receivedIntervals[Math.Max(0, expectedIterations)] = d);
		CancellationTokenSource cancellationTokenSource = new();
		ManualResetEventSlim ms = new();

		Timer timer = timeSystem.CreateTimer(interval, _ =>
		{
			expectedIterations--;
			if (expectedIterations < 0)
			{
				cancellationTokenSource.Cancel();
				ms.Set();
			}
		});
		timer.Start(cancellationTokenSource.Token);
		ms.Wait(1000);

		receivedIntervals.Should().AllBeEquivalentTo(interval);
	}

	[Fact]
	public void CreateTimer_Start_ShouldIterateUntilStopped()
	{
		MockTimeSystem timeSystem = Initialize(
			out TimeSpan interval,
			out int expectedIterations,
			out TimeSpan[] receivedIntervals);
		timeSystem.On.TaskDelay(d =>
			receivedIntervals[Math.Max(0, expectedIterations)] = d);
		Timer? runningTimer = null;
		ManualResetEventSlim ms = new();

		Timer timer = timeSystem.CreateTimer(interval, _ =>
		{
			expectedIterations--;
			if (expectedIterations < 0)
			{
				// ReSharper disable once AccessToModifiedClosure
				runningTimer?.Stop();
				ms.Set();
			}
		});
		runningTimer = timer.Start();
		ms.Wait(1000);

		receivedIntervals.Should().AllBeEquivalentTo(interval);
	}

	[Fact]
	public void CreateTimer_StartTwice_ShouldStopPreviousRun()
	{
		TimeSpan interval = TimeSpan.FromSeconds(10);
		TimeSpan callbackDuration = TimeSpan.FromSeconds(2);
		DateTime startTime = new(2010, 5, 2, 10, 0, 0, DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(TimeProvider.Use(startTime));
		ConcurrentBag<DateTime> requestedTimes = new();
		timeSystem.On.DateTimeRead(t =>
		{
			requestedTimes.Add(t);
		});

		ManualResetEventSlim ms = new();
		int expectedIterations = 10;

		Timer timer = timeSystem.CreateTimer(
			interval,
			token =>
			{
				Thread.Sleep(10);
				token.ThrowIfCancellationRequested();
				timeSystem.Thread.Sleep(callbackDuration);
				expectedIterations--;
				if (expectedIterations < 0)
				{
					ms.Set();
				}
			});
		timer.Start();
		timer.Start();

		ms.Wait(1000);
		timer.Dispose();

		foreach (DateTime requestedTime in requestedTimes)
		{
			// Ensures that only one thread added a Thread.Sleep!
			// Therefore the second Start stopped the first Task.
			requestedTime.Second.Should()
				.Match(s => s % 10 == 2 || s % 10 == 0);
		}
	}

	#region Helpers

	private static MockTimeSystem Initialize(
		out TimeSpan interval,
		out int expectedIterations,
		out TimeSpan[] receivedIntervals)
	{
		Random random = new();
		interval = TimeSpan.FromSeconds(random.Next(1, 100));
		expectedIterations = random.Next(5, 15);
		receivedIntervals = new TimeSpan[expectedIterations];
		return new MockTimeSystem();
	}

	#endregion
}
