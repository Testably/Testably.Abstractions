using System.Threading;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests(true)]
public class TaskDelayCancellationTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task Delay_AlreadyCancelledToken_ShouldThrowOperationCanceledException()
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();
		CancellationToken token = cts.Token;

		async Task Act()
			=> await TimeSystem.Task.Delay(TimeSpan.FromSeconds(30), token);

		await That(Act).Throws<OperationCanceledException>();
	}

	[Test]
	public async Task Delay_CancelledWhilePending_ShouldThrowOperationCanceledException()
	{
		using CancellationTokenSource cts = CancellationTokenSource
			.CreateLinkedTokenSource(TestContext.Current!.Execution.CancellationToken);
		Task delayTask = TimeSystem.Task.Delay(TimeSpan.FromSeconds(30), cts.Token);

		cts.Cancel();

		async Task Act() => await delayTask;

		await That(Act).Throws<OperationCanceledException>();
	}

	[Test]
	public async Task Delay_Zero_ShouldCompleteSuccessfully()
	{
		Exception? exception = await Record.ExceptionAsync(()
			=> TimeSystem.Task.Delay(TimeSpan.Zero,
				TestContext.Current!.Execution.CancellationToken));

		await That(exception).IsNull();
	}
}
