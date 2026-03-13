namespace Testably.Abstractions.Testing.Tests;

public partial class NotificationHandlerExtensionsTests
{
	public sealed class TimeSystemTests
	{
		public MockTimeSystem TimeSystem { get; } = new();

		[Test]
		public async Task Elapsed_WhenAdvanced_ShouldTrigger()
		{
			int notificationCount = 0;

			using IAwaitableCallback<DateTime> onElapsed =
				TimeSystem.On.Elapsed(TimeSpan.FromSeconds(3), _ => notificationCount++);

			for (int i = 0; i < 7; i++)
			{
				TimeSystem.TimeProvider.AdvanceBy(TimeSpan.FromSeconds(1));
			}

			await That(notificationCount).IsEqualTo(5);
		}

		[Test]
		public async Task Elapsed_WhenNotAdvanced_ShouldTimeout()
		{
			int notificationCount = 0;

			using IAwaitableCallback<DateTime> onElapsed =
				TimeSystem.On.Elapsed(TimeSpan.FromMilliseconds(1), _ => notificationCount++);

			Exception? exception = Record.Exception(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				onElapsed.Wait(timeout: 50);
			});

			await That(exception).IsExactly<TimeoutException>();
			await That(notificationCount).IsEqualTo(0);
		}
	}
}
