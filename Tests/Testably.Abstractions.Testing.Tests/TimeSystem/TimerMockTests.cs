using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.FileSystemInitializer;
using Testably.Abstractions.Testing.Tests.TestHelpers;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerMockTests
{
	[SkippableTheory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(2000)]
	public void Change_ValidDueTimeValue_ShouldNotThrowException(int dueTime)
	{
		MockTimeSystem timeSystem = new MockTimeSystem();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(dueTime, 0);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(2000)]
	public void Change_ValidPeriodValue_ShouldNotThrowException(int period)
	{
		MockTimeSystem timeSystem = new MockTimeSystem();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, period);
		});

		exception.Should().BeNull();
	}

	[Fact]
	public void Dispose_ShouldDisposeTimer()
	{
		MockTimeSystem timeSystem = new();
		ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		timer.Dispose();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		});

		exception.Should().BeOfType<ObjectDisposedException>();
	}

#if FEATURE_ASYNC_DISPOSABLE
	[Fact]
	public async Task DisposeAsync_ShouldDisposeTimer()
	{
		MockTimeSystem timeSystem = new();
		ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		await timer.DisposeAsync();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		});

		exception.Should().BeOfType<ObjectDisposedException>();
	}
#endif

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
	public void Exception_WhenSwallowExceptionsIsNotSet_ShouldStopTimer()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(swallowExceptions: false));
		Exception expectedException = new("foo");
		int count = 0;
		using ITimer timer = timeSystem.Timer.New(
			_ =>
			{
				if (++count == 1)
				{
					throw expectedException;
				}
			}, null, 0, 20);

		Exception? exception = Record.Exception(() =>
		{
			Thread.Sleep(10);
			timeSystem.TimerHandler[0].Wait();
		});

		exception.Should().Be(expectedException);
		count.Should().Be(1);
	}

	[Fact]
	public void Exception_WhenSwallowExceptionsIsSet_ShouldContinueTimer()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(swallowExceptions: true));
		Exception expectedException = new("foo");
		int count = 0;
		using ITimer timer = timeSystem.Timer.New(
			_ =>
			{
				if (++count == 1)
				{
					throw expectedException;
				}
			}, null, 0, 20);

		Exception? exception = Record.Exception(() =>
		{
			Thread.Sleep(10);
			timeSystem.TimerHandler[0].Wait();
		});

		exception.Should().Be(expectedException);
		count.Should().BeGreaterThan(1);
	}

	[Fact]
	public void ExecutionCount_ShouldBeIncrementedAndZeroBased()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		ConcurrentBag<int> executionCounter = new();

		using (timeSystem.On.TimerExecuted(d => executionCounter.Add(d.ExecutionCount)))
		{
			timeSystem.Timer.New(_ => { }, null,
				TimeTestHelper.GetRandomInterval(),
				TimeTestHelper.GetRandomInterval());

			timerHandler[0].Wait(10);
		}
		
		executionCounter.OrderBy(x => x).Should()
			.BeEquivalentTo(Enumerable.Range(0, executionCounter.Count));
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

	[SkippableFact]
	public void Wait_Infinite_ShouldBeValidTimeout()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 0, 100);

		Exception? exception = Record.Exception(() =>
		{
			timerHandler[0].Wait(timeout: Timeout.Infinite);
		});

		exception.Should().BeNull();
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

	[SkippableFact]
	public void Wait_WithExecutionCount_ShouldWaitForSpecifiedNumberOfExecutions()
	{
		Skip.If(Test.RunsOnWindows, "Brittle test under Windows on GitHub");

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
