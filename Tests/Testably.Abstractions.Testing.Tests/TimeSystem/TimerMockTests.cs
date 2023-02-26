using System.Threading;
using Testably.Abstractions.Testing.FileSystemInitializer;
using Testably.Abstractions.Testing.Tests.TestHelpers;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerMockTests
{
	[Fact]
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
	public void Exception_ShouldBeIncludedInTimerExecutedNotification()
	{
		TestingException exception = new("foo");
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(swallowExceptions: true));
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		TimerExecution? receivedTimeout = null;

		using (timeSystem.On.TimerExecuted(d => receivedTimeout = d))
		{
			timeSystem.Timer.New(_ => throw exception, null,
				TimeTestHelper.GetRandomInterval(),
				TimeTestHelper.GetRandomInterval());
			try
			{
				timerHandler[0].Wait();
			}
			catch (TestingException)
			{
				// Expect a TestingException to be thrown
			}
		}

		receivedTimeout!.Exception.Should().Be(exception);
	}

	[Fact]
	public void Exception_WhenSwallowExceptionsIsSet_ShouldContinueTimerExecution()
	{
		MockTimeSystem timeSystem = new();
		timeSystem.WithTimerStrategy(
			new TimerStrategy(swallowExceptions: true));
		Exception exception = new("foo");
		int count = 0;
		ManualResetEventSlim ms = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			if (count++ == 1)
			{
				throw exception;
			}

			if (count == 3)
			{
				ms.Set();
			}
		}, null, 0, 20);

		ms.Wait(10000).Should().BeTrue();

		count.Should().BeGreaterThanOrEqualTo(3);
	}

	[Fact]
	public void Exception_WhenSwallowExceptionsIsNotSet_ShouldThrowExceptionOnWait()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(swallowExceptions: false));
		Exception expectedException = new("foo");
		using ITimer timer = timeSystem.Timer.New(
			_ => throw expectedException, null, 0, 20);

		Exception? exception = Record.Exception(() =>
		{
			timeSystem.TimerHandler[0].Wait();
		});

		exception.Should().Be(expectedException);
	}

	[Fact]
	public void New_WithStartOnMockWaitMode_ShouldOnlyStartWhenCallingWait()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ => count++, null, 0, 100);

		Thread.Sleep(10);
		count.Should().Be(0);
		timerHandler[0].Wait();
		count.Should().BeGreaterThan(0);
	}

	[Fact]
	public void Wait_InvalidExecutionCount_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);

		Exception? exception = Record.Exception(() =>
		{
			timerHandler[0].Wait(0);
		});

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("executionCount");
	}

	[Fact]
	public void Wait_InvalidTimeout_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);

		Exception? exception = Record.Exception(() =>
		{
			timerHandler[0].Wait(timeout: -2);
		});

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("timeout");
	}

	[Fact]
	public void Wait_TimeoutExpired_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		ManualResetEventSlim ms = new();

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
			ms.Wait();
		}, null, 0, 100);

		Exception? exception = Record.Exception(() =>
		{
			timerHandler[0].Wait(10, 300);
		});
		ms.Set();

		exception.Should().BeOfType<TimeoutException>();
		count.Should().BeGreaterThan(0);
	}

	[Fact]
	public void Wait_WithExecutionCount_ShouldWaitForSpecifiedNumberOfExecutions()
	{
		int executionCount = 10;
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
		}, null, 0, 100);

		count.Should().Be(0);
		timerHandler[0].Wait(executionCount, callback: t =>
		{
			t.Dispose();
		});
		Thread.Sleep(100);
		count.Should().Be(executionCount);
	}

	private class DummyWaitHandle : WaitHandle
	{
	}
}
