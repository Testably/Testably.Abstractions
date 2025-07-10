using System.Threading;
using Testably.Abstractions.Testing.TimeSystem;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

// ReSharper disable UseAwaitUsing
public class TimerMockTests(ITestOutputHelper testOutputHelper)
{
	[Theory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(2000)]
	public async Task Change_ValidDueTimeValue_ShouldNotThrowException(int dueTime)
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

		await That(exception).IsNull();
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(2000)]
	public async Task Change_ValidPeriodValue_ShouldNotThrowException(int period)
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

		await That(exception).IsNull();
	}

	[Fact]
	public async Task Dispose_ShouldDisposeTimer()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		// ReSharper disable once DisposeOnUsingVariable
		timer.Dispose();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		}

#if NET8_0_OR_GREATER
		await That(Act).DoesNotThrow();
#else
		await That(Act).Throws<ObjectDisposedException>()
			.Whose(x => x.Message, it => it.Satisfies(m => m!.Contains("Cannot access a disposed object.") && m.Contains(nameof(ITimer.Change))));
#endif
	}

	[Fact]
	public async Task Dispose_WithUnknownWaitHandle_ShouldThrowNotSupportedException()
	{
		MockTimeSystem timeSystem = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		using DummyWaitHandle waitHandle = new();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer
				// ReSharper disable once AccessToDisposedClosure
				.Dispose(waitHandle);
		}

		await That(Act).ThrowsExactly<NotSupportedException>().WithMessage($"*{typeof(DummyWaitHandle)}*").AsWildcard();
	}

#if FEATURE_ASYNC_DISPOSABLE
	[Fact]
	public async Task DisposeAsync_ShouldDisposeTimer()
	{
		MockTimeSystem timeSystem = new();
		await using ITimer timer = timeSystem.Timer.New(_ =>
		{
		}, null, 100, 200);
		// ReSharper disable once DisposeOnUsingVariable
		await timer.DisposeAsync();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Change(0, 0);
		}

#if NET8_0_OR_GREATER
		await That(Act).DoesNotThrow();
#else
		await That(Act).Throws<ObjectDisposedException>();
#endif
	}
#endif

	[Fact]
	public async Task Exception_WhenSwallowExceptionsIsNotSet_ShouldStopTimer()
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

		await Task.Delay(10, TestContext.Current.CancellationToken);
		await That(exception).IsEqualTo(expectedException);
		await That(count).IsEqualTo(1);
	}

	[Fact]
	public async Task Exception_WhenSwallowExceptionsIsNotSet_ShouldThrowExceptionOnWait()
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

		await That(exception).IsEqualTo(expectedException);
	}

	[Fact]
	public async Task Exception_WhenSwallowExceptionsIsSet_ShouldContinueTimerExecution()
	{
		MockTimeSystem timeSystem = new();
		timeSystem.WithTimerStrategy(
			new TimerStrategy(swallowExceptions: true));
		Exception exception = new("foo");
		int count = 0;
		using ManualResetEventSlim ms = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				if (count++ == 1)
				{
					throw exception;
				}

				if (count == 3)
				{
					ms.Set();
				}
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}, null, 0, 20);

		await That(ms.Wait(10000, TestContext.Current.CancellationToken)).IsTrue();

		await That(count).IsGreaterThanOrEqualTo(3);
	}

	[Fact]
	public async Task New_WithStartOnMockWaitMode_ShouldOnlyStartWhenCallingWait()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ => count++, null, 0, 100);

		await Task.Delay(10, TestContext.Current.CancellationToken);
		await That(count).IsEqualTo(0);
		timerHandler[0].Wait();
		await That(count).IsGreaterThan(0);
	}

	[Fact]
	public async Task Wait_Infinite_ShouldBeValidTimeout()
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

		await That(exception).IsNull();
	}

	[Fact]
	public async Task Wait_InvalidExecutionCount_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);

		void Act()
		{
			timerHandler[0].Wait(0);
		}

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("executionCount").And
			.WithMessage("Execution count must be greater than zero.").AsPrefix();
	}

	[Fact]
	public async Task Wait_InvalidTimeout_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		using ITimer timer = timeSystem.Timer.New(_ => { }, null, 0, 100);

		void Act()
		{
			timerHandler[0].Wait(timeout: -2);
		}

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("timeout");
	}

	[Fact]
	public async Task Wait_TimeoutExpired_ShouldThrowTimeoutException()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		using ManualResetEventSlim ms = new();

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				count++;
				ms.Wait(TestContext.Current.CancellationToken);
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}, null, 0, 100);

		Exception? exception = Record.Exception(() =>
		{
			timerHandler[0].Wait(10, 300);
		});
		ms.Set();

		await That(exception).IsExactly<TimeoutException>();
		await That(count).IsGreaterThan(0);
	}

	[Fact]
	public async Task Wait_Twice_ShouldContinueExecutionsAfterFirstWait()
	{
		int executionCount = 10;
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
			testOutputHelper.WriteLine($"Execute: {count}");
		}, null, 0, 100);

		testOutputHelper.WriteLine($"Waiting {executionCount} times...");
		timerHandler[0].Wait(executionCount, callback: _ =>
			testOutputHelper.WriteLine("Waiting completed."));
		testOutputHelper.WriteLine($"Waiting {executionCount} times...");
		timerHandler[0].Wait(executionCount, callback: t =>
		{
			testOutputHelper.WriteLine("Waiting completed.");
			testOutputHelper.WriteLine("Disposing...");
			t.Dispose();
			testOutputHelper.WriteLine("Disposed.");
		}, timeout: 10000);
		await That(count).IsGreaterThanOrEqualTo(2 * executionCount);
	}

	[Fact]
	public async Task Wait_WithExecutionCount_ShouldWaitForSpecifiedNumberOfExecutions()
	{
		int executionCount = 10;
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler timerHandler = timeSystem.TimerHandler;

		int count = 0;
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			count++;
			testOutputHelper.WriteLine($"Execute: {count}");
		}, null, 0, 100);

		await That(count).IsEqualTo(0);
		timerHandler[0].Wait(executionCount, callback: t =>
		{
			testOutputHelper.WriteLine("Disposing...");
			t.Dispose();
			testOutputHelper.WriteLine("Disposed.");
		}, timeout: 10000);
		testOutputHelper.WriteLine("Waiting 100ms...");
		await Task.Delay(1000, TestContext.Current.CancellationToken);
		testOutputHelper.WriteLine("Waiting completed.");
		await That(count).IsEqualTo(executionCount);
	}

	private sealed class DummyWaitHandle : WaitHandle;
}
