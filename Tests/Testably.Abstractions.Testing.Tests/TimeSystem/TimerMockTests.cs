using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;
using Xunit.Abstractions;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

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
		MockTimeSystem timeSystem = new();
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
		}, timeout: 1000000);
		_testOutputHelper.WriteLine("Waiting 100ms...");
		Thread.Sleep(1000);
		_testOutputHelper.WriteLine("Waiting completed.");
		count.Should().Be(executionCount);
	}

	private class DummyWaitHandle : WaitHandle
	{
	}
}
