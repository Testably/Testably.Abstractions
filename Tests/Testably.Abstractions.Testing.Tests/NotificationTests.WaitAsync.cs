using System.Threading;

namespace Testably.Abstractions.Testing.Tests;

public partial class NotificationTests
{
	public sealed class WaitAsync
	{
		[Test]
		public async Task AwaitableCallback_Amount_ShouldOnlyReturnAfterNumberOfCallbacks()
		{
			MockTimeSystem timeSystem = new();
			int receivedCount = 0;
			using IAwaitableCallback<TimeSpan> onThreadSleep =
				timeSystem.On.ThreadSleep(t =>
				{
					if (t.TotalMilliseconds > 0)
					{
						receivedCount++;
					}
				});

			_ = Task.Run(async () =>
			{
				await Task.Delay(10, TestContext.Current!.Execution.CancellationToken);
				for (int i = 1; i <= 10; i++)
				{
					timeSystem.Thread.Sleep(i);
					await Task.Delay(1, TestContext.Current!.Execution.CancellationToken);
				}
			}, TestContext.Current!.Execution.CancellationToken);

			TimeSpan[] result = await onThreadSleep.WaitAsync(count: 7,
				cancellationToken: TestContext.Current!.Execution.CancellationToken);
			await That(receivedCount).IsGreaterThanOrEqualTo(7);
			await That(result.Length).IsEqualTo(7);
		}

		[Test]
		public async Task
			AwaitableCallback_CancellationTokenCancelled_ShouldThrowOperationCancelledException()
		{
			MockTimeSystem timeSystem = new();
			bool isCalled = false;
			using ManualResetEventSlim ms = new();
			using IAwaitableCallback<TimeSpan> onThreadSleep =
				timeSystem.On.ThreadSleep(_ =>
				{
					isCalled = true;
				});
			new Thread(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					// Delay larger than cancellation after 10ms
					ms.Wait(TestContext.Current!.Execution.CancellationToken);
					timeSystem.Thread.Sleep(1);
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}).Start();

			Exception? exception = await Record.ExceptionAsync(async () =>
			{
				using CancellationTokenSource cts = new();
				cts.CancelAfter(10);
				await onThreadSleep.WaitAsync(cancellationToken: cts.Token);
			});

			await That(exception).IsExactly<OperationCanceledException>();
			await That(isCalled).IsFalse();
			ms.Set();
		}

		[Test]
		public async Task AwaitableCallback_ShouldWaitForCallbackExecution()
		{
			using ManualResetEventSlim ms = new();
			try
			{
				MockTimeSystem timeSystem = new();
				bool isCalled = false;
				using IAwaitableCallback<TimeSpan> onThreadSleep =
					timeSystem.On.ThreadSleep(_ =>
					{
						isCalled = true;
					});

				_ = Task.Run(async () =>
				{
					// ReSharper disable once AccessToDisposedClosure
					try
					{
						while (!ms.IsSet)
						{
							timeSystem.Thread.Sleep(1);
							await Task.Delay(1, TestContext.Current!.Execution.CancellationToken);
						}
					}
					catch (ObjectDisposedException)
					{
						// Ignore any ObjectDisposedException
					}
				}, TestContext.Current!.Execution.CancellationToken);

				await onThreadSleep.WaitAsync(
					cancellationToken: TestContext.Current!.Execution.CancellationToken);
				await That(isCalled).IsTrue();
			}
			finally
			{
				ms.Set();
			}
		}

		[Test]
		public async Task
			AwaitableCallback_TimeoutExpired_ShouldThrowOperationCancelledException()
		{
			MockTimeSystem timeSystem = new();
			bool isCalled = false;
			using ManualResetEventSlim ms = new();
			using IAwaitableCallback<TimeSpan> onThreadSleep =
				timeSystem.On.ThreadSleep(_ =>
				{
					isCalled = true;
				});
			new Thread(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					// Delay larger than timeout of 10ms
					ms.Wait(TestContext.Current!.Execution.CancellationToken);
					timeSystem.Thread.Sleep(1);
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}).Start();

			Exception? exception = await Record.ExceptionAsync(async () =>
			{
				await onThreadSleep.WaitAsync(1, TimeSpan.FromMilliseconds(10),
					TestContext.Current!.Execution.CancellationToken);
			});

			await That(exception).IsExactly<OperationCanceledException>();
			await That(isCalled).IsFalse();
			ms.Set();
		}

		[Test]
		public async Task
			AwaitableCallback_WaitAsync_AfterDispose_ShouldThrowObjectDisposedException()
		{
			MockTimeSystem timeSystem = new();
			IAwaitableCallback<TimeSpan> onThreadSleep =
				timeSystem.On.ThreadSleep();

			onThreadSleep.Dispose();

			async Task Act()
			{
				await onThreadSleep.WaitAsync(timeout: 20000,
					cancellationToken: TestContext.Current!.Execution.CancellationToken);
			}

			await That(Act).ThrowsExactly<ObjectDisposedException>()
				.WithMessage("The awaitable callback is already disposed.");
		}

		[Test]
		public async Task AwaitableCallback_WaitedPreviously_ShouldWaitAgainForCallbackExecution()
		{
			int secondThreadMilliseconds = 42;
			int firstThreadMilliseconds = secondThreadMilliseconds + 1;
			using ManualResetEventSlim ms = new();
			MockTimeSystem timeSystem = new();
			using ManualResetEventSlim listening = new();
			using IAwaitableCallback<TimeSpan> onThreadSleep =
				timeSystem.On.ThreadSleep();

			_ = Task.Delay(50, TestContext.Current!.Execution.CancellationToken)
				.ContinueWith(_ => timeSystem.Thread.Sleep(firstThreadMilliseconds),
					TestContext.Current!.Execution.CancellationToken)
				.ContinueWith(
					async _ => await Task.Delay(20, TestContext.Current!.Execution.CancellationToken),
					TestContext.Current!.Execution.CancellationToken)
				.ContinueWith(_ => timeSystem.Thread.Sleep(secondThreadMilliseconds),
					TestContext.Current!.Execution.CancellationToken);

			TimeSpan[] result1 =
				await onThreadSleep.WaitAsync(
					cancellationToken: TestContext.Current!.Execution.CancellationToken);
			TimeSpan[] result2 =
				await onThreadSleep.WaitAsync(
					cancellationToken: TestContext.Current!.Execution.CancellationToken);
			await That(result1).HasSingle().Which
				.Satisfies(f => f.Milliseconds == firstThreadMilliseconds);
			await That(result2).HasSingle().Which
				.Satisfies(f => f.Milliseconds == secondThreadMilliseconds);
		}

		[Test]
		[AutoArguments]
		public async Task Execute_ShouldBeExecutedBeforeWait(int milliseconds)
		{
			MockTimeSystem timeSystem = new();
			int receivedMilliseconds = -1;

			using IAwaitableCallback<TimeSpan> onThreadSleep = timeSystem.On
				.ThreadSleep(t =>
				{
					receivedMilliseconds = (int)t.TotalMilliseconds;
				});
			timeSystem.Thread.Sleep(milliseconds);
			await onThreadSleep.WaitAsync(cancellationToken: TestContext.Current!.Execution.CancellationToken);
			await That(receivedMilliseconds).IsEqualTo(milliseconds);
		}

		[Test]
		public async Task ExecuteWhileWaiting_ShouldExecuteCallback()
		{
			MockTimeSystem timeSystem = new();

			using IAwaitableCallback<TimeSpan> onThreadSleep = timeSystem.On
				.ThreadSleep();

			timeSystem.Thread.Sleep(10);
			TimeSpan[] result =
				await onThreadSleep.WaitAsync(
					cancellationToken: TestContext.Current!.Execution.CancellationToken);
			await That(result).HasSingle().Which.Satisfies(f => f.Milliseconds == 10);
		}
	}
}
