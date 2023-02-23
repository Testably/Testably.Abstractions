using System.Threading;
using Testably.Abstractions.Testing.Tests.TestHelpers;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

[Collection(nameof(Timer))]
public class NotificationHandlerTests
{
	[Fact]
	public void
		OnDateTimeRead_DisposedCallback_ShouldNotBeCalled()
	{
		DateTime expectedTime = TimeTestHelper.GetRandomTime(DateTimeKind.Local);
		MockTimeSystem timeSystem = new(expectedTime);
		DateTime? receivedTime = null;
		IDisposable disposable = timeSystem.On.DateTimeRead(d => receivedTime = d);

		disposable.Dispose();
		_ = timeSystem.DateTime.Now;

		receivedTime.Should().BeNull();
	}

	[Fact]
	public void
		OnDateTimeRead_MultipleCallbacks_DisposeOne_ShouldCallOtherCallbacks()
	{
		DateTime expectedTime = TimeTestHelper.GetRandomTime(DateTimeKind.Local);
		MockTimeSystem timeSystem = new(expectedTime);
		DateTime? receivedTime1 = null;
		DateTime? receivedTime2 = null;

		using (timeSystem.On.DateTimeRead(d => receivedTime1 = d))
		{
			timeSystem.On.DateTimeRead(d => receivedTime2 = d).Dispose();
			_ = timeSystem.DateTime.Now;
		}

		receivedTime1.Should().Be(expectedTime);
		receivedTime2.Should().BeNull();
	}

	[Fact]
	public void
		OnDateTimeRead_MultipleCallbacks_ShouldAllBeCalled()
	{
		DateTime expectedTime = TimeTestHelper.GetRandomTime(DateTimeKind.Local);
		MockTimeSystem timeSystem = new(expectedTime);
		DateTime? receivedTime1 = null;
		DateTime? receivedTime2 = null;

		using (timeSystem.On.DateTimeRead(d => receivedTime1 = d))
		{
			using (timeSystem.On.DateTimeRead(d => receivedTime2 = d))
			{
				_ = timeSystem.DateTime.Now;
			}
		}

		receivedTime1.Should().Be(expectedTime);
		receivedTime2.Should().Be(expectedTime);
	}

	[Fact]
	public void
		OnDateTimeRead_Today_ShouldExecuteCallbackWithCorrectParameter()
	{
		DateTime expectedTime = TimeTestHelper.GetRandomTime().Date;
		MockTimeSystem timeSystem = new(expectedTime);
		DateTime? receivedTime = null;

		using (timeSystem.On.DateTimeRead(d => receivedTime = d))
		{
			_ = timeSystem.DateTime.Today;
		}

		receivedTime.Should().Be(expectedTime);
	}

	[Fact]
	public void
		OnDateTimeRead_UtcNow_ShouldExecuteCallbackWithCorrectParameter()
	{
		DateTime expectedTime = TimeTestHelper.GetRandomTime(DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(expectedTime);
		DateTime? receivedTime = null;

		using (timeSystem.On.DateTimeRead(d => receivedTime = d))
		{
			_ = timeSystem.DateTime.UtcNow;
		}

		receivedTime.Should().Be(expectedTime);
	}

	[Fact]
	public void
		OnTaskDelay_DisposedCallback_ShouldNotBeCalled()
	{
		int millisecondsDelay = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedDelay = null;
		IDisposable disposable = timeSystem.On.TaskDelay(d => receivedDelay = d);

		disposable.Dispose();
		timeSystem.Task.Delay(millisecondsDelay);

		receivedDelay.Should().BeNull();
	}

	[Fact]
	public void
		OnTaskDelay_MultipleCallbacks_DisposeOne_ShouldCallOtherCallbacks()
	{
		TimeSpan expectedDelay = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedDelay1 = null;
		TimeSpan? receivedDelay2 = null;

		using (timeSystem.On.TaskDelay(d => receivedDelay1 = d))
		{
			timeSystem.On.TaskDelay(d => receivedDelay2 = d).Dispose();
			timeSystem.Task.Delay(expectedDelay);
		}

		receivedDelay1.Should().Be(expectedDelay);
		receivedDelay2.Should().BeNull();
	}

	[Fact]
	public void
		OnTaskDelay_MultipleCallbacks_ShouldAllBeCalled()
	{
		TimeSpan expectedDelay = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedDelay1 = null;
		TimeSpan? receivedDelay2 = null;

		using (timeSystem.On.TaskDelay(d => receivedDelay1 = d))
		{
			using (timeSystem.On.TaskDelay(d => receivedDelay2 = d))
			{
				timeSystem.Task.Delay(expectedDelay);
			}
		}

		receivedDelay1.Should().Be(expectedDelay);
		receivedDelay2.Should().Be(expectedDelay);
	}

	[Fact]
	public void
		OnTaskDelay_WithMillisecondsAndWithCancellationToken_ShouldExecuteCallbackWithCorrectParameter()
	{
		int millisecondsDelay = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedDelay = TimeSpan.Zero;

		using (timeSystem.On.TaskDelay(d => receivedDelay = d))
		{
			timeSystem.Task.Delay(millisecondsDelay, CancellationToken.None);
		}

		receivedDelay.TotalMilliseconds.Should().Be(millisecondsDelay);
	}

	[Fact]
	public void
		OnTaskDelay_WithMillisecondsAndWithoutCancellationToken_ShouldExecuteCallbackWithCorrectParameter()
	{
		int millisecondsDelay = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedDelay = TimeSpan.Zero;

		using (timeSystem.On.TaskDelay(d => receivedDelay = d))
		{
			timeSystem.Task.Delay(millisecondsDelay);
		}

		receivedDelay.TotalMilliseconds.Should().Be(millisecondsDelay);
	}

	[Fact]
	public void
		OnTaskDelay_WithTimeSpanAndWithCancellationToken_ShouldExecuteCallbackWithCorrectParameter()
	{
		TimeSpan expectedDelay = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedDelay = TimeSpan.Zero;

		using (timeSystem.On.TaskDelay(d => receivedDelay = d))
		{
			timeSystem.Task.Delay(expectedDelay, CancellationToken.None);
		}

		receivedDelay.Should().Be(expectedDelay);
	}

	[Fact]
	public void
		OnTaskDelay_WithTimeSpanAndWithoutCancellationToken_ShouldExecuteCallbackWithCorrectParameter()
	{
		TimeSpan expectedDelay = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedDelay = TimeSpan.Zero;

		using (timeSystem.On.TaskDelay(d => receivedDelay = d))
		{
			timeSystem.Task.Delay(expectedDelay);
		}

		receivedDelay.Should().Be(expectedDelay);
	}

	[Fact]
	public void
		OnThreadSleep_DisposedCallback_ShouldNotBeCalled()
	{
		int millisecondsTimeout = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedTimeout = null;
		IDisposable disposable = timeSystem.On.ThreadSleep(d => receivedTimeout = d);

		disposable.Dispose();
		timeSystem.Thread.Sleep(millisecondsTimeout);

		receivedTimeout.Should().BeNull();
	}

	[Fact]
	public void
		OnThreadSleep_MultipleCallbacks_DisposeOne_ShouldCallOtherCallbacks()
	{
		TimeSpan expectedTimeout = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedTimeout1 = null;
		TimeSpan? receivedTimeout2 = null;

		using (timeSystem.On.ThreadSleep(d => receivedTimeout1 = d))
		{
			timeSystem.On.ThreadSleep(d => receivedTimeout2 = d).Dispose();
			timeSystem.Thread.Sleep(expectedTimeout);
		}

		receivedTimeout1.Should().Be(expectedTimeout);
		receivedTimeout2.Should().BeNull();
	}

	[Fact]
	public void
		OnThreadSleep_MultipleCallbacks_ShouldAllBeCalled()
	{
		TimeSpan expectedTimeout = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedTimeout1 = null;
		TimeSpan? receivedTimeout2 = null;

		using (timeSystem.On.ThreadSleep(d => receivedTimeout1 = d))
		{
			using (timeSystem.On.ThreadSleep(d => receivedTimeout2 = d))
			{
				timeSystem.Thread.Sleep(expectedTimeout);
			}
		}

		receivedTimeout1.Should().Be(expectedTimeout);
		receivedTimeout2.Should().Be(expectedTimeout);
	}

	[Fact]
	public void
		OnThreadSleep_WithMilliseconds_ShouldExecuteCallbackWithCorrectParameter()
	{
		int millisecondsTimeout = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedTimeout = TimeSpan.Zero;

		using (timeSystem.On.ThreadSleep(d => receivedTimeout = d))
		{
			timeSystem.Thread.Sleep(millisecondsTimeout);
		}

		receivedTimeout.TotalMilliseconds.Should().Be(millisecondsTimeout);
	}

	[Fact]
	public void
		OnThreadSleep_WithTimeSpan_ShouldExecuteCallbackWithCorrectParameter()
	{
		TimeSpan expectedTimeout = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedTimeout = TimeSpan.Zero;

		using (timeSystem.On.ThreadSleep(d => receivedTimeout = d))
		{
			timeSystem.Thread.Sleep(expectedTimeout);
		}

		receivedTimeout.Should().Be(expectedTimeout);
	}

	[Fact]
	public void
		OnTimerExecuted_DisposedCallback_ShouldNotBeCalled()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		TimerExecution? receivedValue = null;
		IDisposable disposable = timeSystem.On.TimerExecuted(d => receivedValue = d);

		disposable.Dispose();
		timeSystem.Timer.New(_ => { }, null, TimeTestHelper.GetRandomInterval(),
			TimeTestHelper.GetRandomInterval());

		timerHandler[0].Wait();
		receivedValue.Should().BeNull();
	}

	[Fact]
	public void
		OnTimerExecuted_MultipleCallbacks_DisposeOne_ShouldCallOtherCallbacks()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		TimerExecution? receivedTimeout1 = null;
		TimerExecution? receivedTimeout2 = null;

		using (timeSystem.On.TimerExecuted(d => receivedTimeout1 = d))
		{
			timeSystem.On.TimerExecuted(d => receivedTimeout2 = d).Dispose();
			timeSystem.Timer.New(_ => { }, null, TimeTestHelper.GetRandomInterval(),
				TimeTestHelper.GetRandomInterval());
			timerHandler[0].Wait();
		}

		receivedTimeout1.Should().NotBeNull();
		receivedTimeout2.Should().BeNull();
	}

	[Fact]
	public void
		OnTimerExecuted_MultipleCallbacks_ShouldAllBeCalled()
	{
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		TimerExecution? receivedTimeout1 = null;
		TimerExecution? receivedTimeout2 = null;

		using (timeSystem.On.TimerExecuted(d => receivedTimeout1 = d))
		{
			using (timeSystem.On.TimerExecuted(d => receivedTimeout2 = d))
			{
				timeSystem.Timer.New(_ => { }, null, TimeTestHelper.GetRandomInterval(),
					TimeTestHelper.GetRandomInterval());
				timerHandler[0].Wait();
			}
		}

		receivedTimeout1.Should().NotBeNull();
		receivedTimeout2.Should().NotBeNull();
	}

	[Fact]
	public void
		OnTimerExecuted_WithMilliseconds_ShouldExecuteCallbackWithCorrectParameter()
	{
		int millisecondsTimeout = new Random().Next();
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		TimerExecution? receivedValue = null;
		DateTime now = timeSystem.DateTime.UtcNow;

		using (timeSystem.On.TimerExecuted(d => receivedValue = d))
		{
			timeSystem.Timer.New(_ => { }, null, millisecondsTimeout, 0);
			timerHandler[0].Wait();
		}

		TimeSpan difference = receivedValue!.Time - now;
		difference.Should().Be(TimeSpan.FromMilliseconds(millisecondsTimeout));
	}

	[Fact]
	public void
		OnTimerExecuted_WithTimeSpan_ShouldExecuteCallbackWithCorrectParameter()
	{
		TimeSpan expectedTimeout = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		ITimerHandler timerHandler = timeSystem.TimerHandler;
		TimerExecution? receivedValue = null;
		DateTime now = timeSystem.DateTime.UtcNow;

		using (timeSystem.On.TimerExecuted(d => receivedValue = d))
		{
			timeSystem.Timer.New(_ => { }, null, expectedTimeout, TimeSpan.Zero);
			timerHandler[0].Wait();
		}

		TimeSpan difference = receivedValue!.Time - now;
		difference.Should().Be(expectedTimeout);
	}
}
