using System.Threading;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;
using Xunit.Abstractions;
#if FEATURE_ASYNC_DISPOSABLE
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

// ReSharper disable AccessToDisposedClosure
public class TimerMockTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public TimerMockTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[SkippableTheory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(2000)]
	public void Change_ValidDueTimeValue_ShouldNotThrowException(int dueTime)
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
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
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);

		Exception? exception = Record.Exception(() =>
		{
			timer.Change(0, period);
		});

		exception.Should().BeNull();
	}

	[Fact]
	public void Dispose_ShouldDisposeTimer()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		timer.Dispose();

		Exception? exception = Record.Exception(() =>
		{
			timer.Change(0, 0);
		});

		exception.Should().BeOfType<ObjectDisposedException>()
			.Which.Message.Should().ContainAll(
				"Cannot access a disposed object.",
				nameof(ITimer.Change));
	}

#if FEATURE_ASYNC_DISPOSABLE
	[Fact]
	public async Task DisposeAsync_ShouldDisposeTimer()
	{
		MockTimeSystem timeSystem = new();
		await using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		await timer.DisposeAsync();

		Exception? exception = Record.Exception(() =>
		{
			timer.Change(0, 0);
		});

		exception.Should().BeOfType<ObjectDisposedException>();
	}
#endif

	[Fact]
	public void Dispose_WithUnknownWaitHandle_ShouldThrowNotSupportedException()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using DummyWaitHandle waitHandle = new();

		Exception? exception = Record.Exception(() =>
		{
			timer.Dispose(waitHandle);
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should().Contain(typeof(DummyWaitHandle).ToString());
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
			.WithTimerStrategy(new TimerStrategy(
				TimerMode.StartOnMockWait,
				swallowExceptions: false));
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
			.WithTimerStrategy(new TimerStrategy(
				TimerMode.StartOnMockWait,
				swallowExceptions: false));
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
			timeSystem.TimerHandler[0].Wait();
		});

		Thread.Sleep(10);
		exception.Should().Be(expectedException);
		count.Should().Be(1);
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
		exception!.Message.Should().Contain("Execution count must be greater than zero.");
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

	[Fact]
	public void Wait_Twice_ShouldContinueExecutionsAfterFirstWait()
	{
		int executionCount = 10;
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
			_testOutputHelper.WriteLine($"Execute: {count}");
		}, null, 0, 100);

		_testOutputHelper.WriteLine($"Waiting {executionCount} times...");
		timerHandler[0].Wait(executionCount, callback: _ =>
			_testOutputHelper.WriteLine("Waiting completed."));
		_testOutputHelper.WriteLine($"Waiting {executionCount} times...");
		timerHandler[0].Wait(executionCount, callback: t =>
		{
			_testOutputHelper.WriteLine("Waiting completed.");
			_testOutputHelper.WriteLine("Disposing...");
			t.Dispose();
			_testOutputHelper.WriteLine("Disposed.");
		}, timeout: 10000);
		count.Should().BeGreaterOrEqualTo(2 * executionCount);
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
			_testOutputHelper.WriteLine($"Execute: {count}");
		}, null, 0, 100);

		count.Should().Be(0);
		timerHandler[0].Wait(executionCount, callback: t =>
		{
			_testOutputHelper.WriteLine("Disposing...");
			t.Dispose();
			_testOutputHelper.WriteLine("Disposed.");
		}, timeout: 10000);
		_testOutputHelper.WriteLine("Waiting 100ms...");
		Thread.Sleep(1000);
		_testOutputHelper.WriteLine("Waiting completed.");
		count.Should().Be(executionCount);
	}

	private class DummyWaitHandle : WaitHandle
	{
	}
}
