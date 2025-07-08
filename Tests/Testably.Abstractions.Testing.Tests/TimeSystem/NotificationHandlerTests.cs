using System.Threading;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class NotificationHandlerTests
{
	[Fact]
	public async Task OnDateTimeRead_DisposedCallback_ShouldNotBeCalled()
	{
		DateTime expectedTime = TimeTestHelper.GetRandomTime(DateTimeKind.Local);
		MockTimeSystem timeSystem = new(expectedTime);
		DateTime? receivedTime = null;
		IDisposable disposable = timeSystem.On.DateTimeRead(d => receivedTime = d);

		disposable.Dispose();
		_ = timeSystem.DateTime.Now;

		await That(receivedTime).IsNull();
	}

	[Fact]
	public async Task OnDateTimeRead_MultipleCallbacks_DisposeOne_ShouldCallOtherCallbacks()
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

		await That(receivedTime1).IsEqualTo(expectedTime);
		await That(receivedTime2).IsNull();
	}

	[Fact]
	public async Task OnDateTimeRead_MultipleCallbacks_ShouldAllBeCalled()
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

		await That(receivedTime1).IsEqualTo(expectedTime);
		await That(receivedTime2).IsEqualTo(expectedTime);
	}

	[Fact]
	public async Task OnDateTimeRead_Today_ShouldExecuteCallbackWithCorrectParameter()
	{
		DateTime expectedTime = TimeTestHelper.GetRandomTime().Date;
		MockTimeSystem timeSystem = new(expectedTime);
		DateTime? receivedTime = null;

		using (timeSystem.On.DateTimeRead(d => receivedTime = d))
		{
			_ = timeSystem.DateTime.Today;
		}

		await That(receivedTime).IsEqualTo(expectedTime);
	}

	[Fact]
	public async Task OnDateTimeRead_UtcNow_ShouldExecuteCallbackWithCorrectParameter()
	{
		DateTime expectedTime = TimeTestHelper.GetRandomTime(DateTimeKind.Utc);
		MockTimeSystem timeSystem = new(expectedTime);
		DateTime? receivedTime = null;

		using (timeSystem.On.DateTimeRead(d => receivedTime = d))
		{
			_ = timeSystem.DateTime.UtcNow;
		}

		await That(receivedTime).IsEqualTo(expectedTime);
	}

	[Fact]
	public async Task OnTaskDelay_DisposedCallback_ShouldNotBeCalled()
	{
		int millisecondsDelay = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedDelay = null;
		IDisposable disposable = timeSystem.On.TaskDelay(d => receivedDelay = d);

		disposable.Dispose();
		_ = timeSystem.Task.Delay(millisecondsDelay, TestContext.Current.CancellationToken);

		await That(receivedDelay).IsNull();
	}

	[Fact]
	public async Task OnTaskDelay_MultipleCallbacks_DisposeOne_ShouldCallOtherCallbacks()
	{
		TimeSpan expectedDelay = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedDelay1 = null;
		TimeSpan? receivedDelay2 = null;

		using (timeSystem.On.TaskDelay(d => receivedDelay1 = d))
		{
			timeSystem.On.TaskDelay(d => receivedDelay2 = d).Dispose();
			_ = timeSystem.Task.Delay(expectedDelay, TestContext.Current.CancellationToken);
		}

		await That(receivedDelay1).IsEqualTo(expectedDelay);
		await That(receivedDelay2).IsNull();
	}

	[Fact]
	public async Task OnTaskDelay_MultipleCallbacks_ShouldAllBeCalled()
	{
		TimeSpan expectedDelay = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedDelay1 = null;
		TimeSpan? receivedDelay2 = null;

		using (timeSystem.On.TaskDelay(d => receivedDelay1 = d))
		{
			using (timeSystem.On.TaskDelay(d => receivedDelay2 = d))
			{
				_ = timeSystem.Task.Delay(expectedDelay, TestContext.Current.CancellationToken);
			}
		}

		await That(receivedDelay1).IsEqualTo(expectedDelay);
		await That(receivedDelay2).IsEqualTo(expectedDelay);
	}

	[Fact]
	public async Task OnTaskDelay_WithMillisecondsAndWithCancellationToken_ShouldExecuteCallbackWithCorrectParameter()
	{
		int millisecondsDelay = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedDelay = TimeSpan.Zero;

		using (timeSystem.On.TaskDelay(d => receivedDelay = d))
		{
			_ = timeSystem.Task.Delay(millisecondsDelay, CancellationToken.None);
		}

		await That(receivedDelay.TotalMilliseconds).IsEqualTo(millisecondsDelay);
	}

	[Fact]
	public async Task OnTaskDelay_WithMillisecondsAndWithoutCancellationToken_ShouldExecuteCallbackWithCorrectParameter()
	{
		int millisecondsDelay = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedDelay = TimeSpan.Zero;

		using (timeSystem.On.TaskDelay(d => receivedDelay = d))
		{
			_ = timeSystem.Task.Delay(millisecondsDelay, TestContext.Current.CancellationToken);
		}

		await That(receivedDelay.TotalMilliseconds).IsEqualTo(millisecondsDelay);
	}

	[Fact]
	public async Task OnTaskDelay_WithTimeSpanAndWithCancellationToken_ShouldExecuteCallbackWithCorrectParameter()
	{
		TimeSpan expectedDelay = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedDelay = TimeSpan.Zero;

		using (timeSystem.On.TaskDelay(d => receivedDelay = d))
		{
			_ = timeSystem.Task.Delay(expectedDelay, CancellationToken.None);
		}

		await That(receivedDelay).IsEqualTo(expectedDelay);
	}

	[Fact]
	public async Task OnTaskDelay_WithTimeSpanAndWithoutCancellationToken_ShouldExecuteCallbackWithCorrectParameter()
	{
		TimeSpan expectedDelay = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedDelay = TimeSpan.Zero;

		using (timeSystem.On.TaskDelay(d => receivedDelay = d))
		{
			_ = timeSystem.Task.Delay(expectedDelay, TestContext.Current.CancellationToken);
		}

		await That(receivedDelay).IsEqualTo(expectedDelay);
	}

	[Fact]
	public async Task OnThreadSleep_DisposedCallback_ShouldNotBeCalled()
	{
		int millisecondsTimeout = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedTimeout = null;
		IDisposable disposable = timeSystem.On.ThreadSleep(d => receivedTimeout = d);

		disposable.Dispose();
		timeSystem.Thread.Sleep(millisecondsTimeout);

		await That(receivedTimeout).IsNull();
	}

	[Fact]
	public async Task OnThreadSleep_MultipleCallbacks_DisposeOne_ShouldCallOtherCallbacks()
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

		await That(receivedTimeout1).IsEqualTo(expectedTimeout);
		await That(receivedTimeout2).IsNull();
	}

	[Fact]
	public async Task OnThreadSleep_MultipleCallbacks_ShouldAllBeCalled()
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

		await That(receivedTimeout1).IsEqualTo(expectedTimeout);
		await That(receivedTimeout2).IsEqualTo(expectedTimeout);
	}

	[Fact]
	public async Task OnThreadSleep_WithMilliseconds_ShouldExecuteCallbackWithCorrectParameter()
	{
		int millisecondsTimeout = new Random().Next();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedTimeout = TimeSpan.Zero;

		using (timeSystem.On.ThreadSleep(d => receivedTimeout = d))
		{
			timeSystem.Thread.Sleep(millisecondsTimeout);
		}

		await That(receivedTimeout.TotalMilliseconds).IsEqualTo(millisecondsTimeout);
	}

	[Fact]
	public async Task OnThreadSleep_WithTimeSpan_ShouldExecuteCallbackWithCorrectParameter()
	{
		TimeSpan expectedTimeout = TimeTestHelper.GetRandomInterval();
		MockTimeSystem timeSystem = new();
		TimeSpan receivedTimeout = TimeSpan.Zero;

		using (timeSystem.On.ThreadSleep(d => receivedTimeout = d))
		{
			timeSystem.Thread.Sleep(expectedTimeout);
		}

		await That(receivedTimeout).IsEqualTo(expectedTimeout);
	}
}
