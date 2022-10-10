using System.Threading;

namespace Testably.Abstractions.Extensions.Tests;

public class TimeSystemExtensionsTaskTests
{
	[Fact]
	[Trait(nameof(Extensions), nameof(ITimeSystem))]
	public void TryDelay_CancelledCancellationToken_ShouldReturnFalse()
	{
		TimeSpan delay = TimeSpan.FromSeconds(1.0);
		TimeSystemMock timeSystem = new();
		CancellationTokenSource cts = new();
		cts.Cancel();

		bool result = timeSystem.Task.TryDelay(delay, cts.Token);

		result.Should().BeFalse();
	}

	[Fact]
	[Trait(nameof(Extensions), nameof(ITimeSystem))]
	public void TryDelay_OperationCanceledException_ShouldReturnFalse()
	{
		TimeSpan delay = TimeSpan.FromSeconds(1.0);
		OperationCanceledException exception = new();
		TimeSystemMock timeSystem = new();
		timeSystem.On.TaskDelay(_ => throw exception);

		bool result = timeSystem.Task.TryDelay(delay);

		result.Should().BeFalse();
	}

	[Fact]
	[Trait(nameof(Extensions), nameof(ITimeSystem))]
	public void TryDelay_ShouldDelayBySpecifiedInterval()
	{
		TimeSpan expectedDelay = TimeSpan.FromSeconds(new Random().Next(0, 3600));
		TimeSystemMock timeSystem = new();
		TimeSpan? receivedDelay = null;
		timeSystem.On.TaskDelay(d => receivedDelay = d);

		bool result = timeSystem.Task.TryDelay(expectedDelay);

		result.Should().BeTrue();
		receivedDelay.Should().Be(expectedDelay);
	}
}