using FluentAssertions;
using System;
using System.Threading;
using Testably.Abstractions.Testing;
using Xunit;

namespace Timer.Tests;

public class TimeSystemExtensionsTests
{
	[Fact]
	public void TryDelay_CancelledCancellationToken_ShouldReturnFalse()
	{
		TimeSpan delay = TimeSpan.FromSeconds(1.0);
		MockTimeSystem timeSystem = new();
		CancellationTokenSource cts = new();
		cts.Cancel();

		bool result = timeSystem.Task.TryDelay(delay, cts.Token);

		result.Should().BeFalse();
	}

	[Fact]
	public void TryDelay_OperationCanceledException_ShouldReturnFalse()
	{
		TimeSpan delay = TimeSpan.FromSeconds(1.0);
		OperationCanceledException exception = new();
		MockTimeSystem timeSystem = new();
		timeSystem.On.TaskDelay(_ => throw exception);

		bool result = timeSystem.Task.TryDelay(delay);

		result.Should().BeFalse();
	}

	[Fact]
	public void TryDelay_ShouldDelayBySpecifiedInterval()
	{
		TimeSpan expectedDelay = TimeSpan.FromSeconds(new Random().Next(0, 3600));
		MockTimeSystem timeSystem = new();
		TimeSpan? receivedDelay = null;
		timeSystem.On.TaskDelay(d => receivedDelay = d);

		bool result = timeSystem.Task.TryDelay(expectedDelay);

		result.Should().BeTrue();
		receivedDelay.Should().Be(expectedDelay);
	}
}
