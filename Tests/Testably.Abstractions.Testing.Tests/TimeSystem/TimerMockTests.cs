﻿using System.Threading;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerMockTests
{
	[SkippableFact]
	public void Dispose_WithUnknownWaitHandle_ShouldThrowNotSupportedException()
	{
		MockTimeSystem timeSystem = new();
		ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using DummyWaitHandle waitHandle = new();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Dispose(waitHandle);
		});

		exception.Should().BeOfType<NotSupportedException>();
	}

	[Fact]
	public void New_WithStartOnMockWaitMode_ShouldOnlyStartWhenCallingWait()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler =
			timeSystem.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ => count++, null, 0, 100);

		Thread.Sleep(10);
		count.Should().Be(0);
		timerHandler[0].Wait();
		count.Should().BeGreaterThan(0);
	}

	[Fact]
	public void Wait_TimeoutExpired_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.WithTimerStrategy(TimerStrategy.Default);

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
			Thread.Sleep(100);
		}, null, 0, 100);

		Exception? exception = Record.Exception(() =>
		{
			timerHandler[0].Wait(10, 300);
		});

		exception.Should().BeOfType<TimeoutException>();
		count.Should().BeGreaterThan(1);
	}

	[Theory]
	[AutoData]
	public void Wait_WithExecutionCount_ShouldWaitForSpecifiedNumberOfExecutions(int executionCount)
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler =
			timeSystem.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
		}, null, 0, 100);

		Thread.Sleep(10);
		count.Should().Be(0);
		timerHandler[0].Wait(executionCount, callback: t => t.Dispose());
		count.Should().Be(executionCount);
	}

	private class DummyWaitHandle : WaitHandle
	{
	}
}
