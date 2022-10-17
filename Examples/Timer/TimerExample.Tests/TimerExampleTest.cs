using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.Threading;
using Testably.Abstractions.Testing;
using Xunit;

namespace TimerExample.Tests;

public class TimerExampleTest
{
	/// <summary>
	///     Tests that the <paramref name="thrownException" /> is caught and forwarded to the
	///     error handler of the <see cref="SynchronizationTimer" />.
	/// </summary>
	[Theory]
	[AutoData]
	public void Exception_ShouldBeHandled(Exception thrownException)
	{
		TimeSystemMock timeSystem = new();
		Exception? receivedException = null;
		// Add a fallback timeout that will fail the test latest after 1 second
		CancellationTokenSource cancellationTokenSource = new(1000);
		CancellationToken cancellationToken = cancellationTokenSource.Token;
		using SynchronizationTimer sut = new(
			timeSystem,
			TimeSpan.FromSeconds(3),
			_ =>
			{
				// Add a delay (e.g. `Thread.Sleep(2000);`) to fail the test due to the fallback timeout.
				throw thrownException;
			},
			ex =>
			{
				receivedException = ex;
				cancellationTokenSource.Cancel();
			});

		sut.Start(cancellationToken);

		cancellationToken.WaitHandle.WaitOne();
		receivedException.Should().Be(thrownException);
	}

	/// <summary>
	///     Tests that the callback is executed repeatedly (at least <paramref name="cancelAfterIterations" /> times).
	/// </summary>
	[Theory]
	[InlineData(5)]
	[InlineData(200)]
	public void Execute_ShouldBeCalledRepeatedly(int cancelAfterIterations)
	{
		int receivedIterations = 0;
		// Add a fallback timeout that will fail the test latest after 1 second
		CancellationTokenSource cancellationTokenSource = new(1000);
		CancellationToken cancellationToken = cancellationTokenSource.Token;
		TimeSystemMock timeSystem = new();
		using SynchronizationTimer sut = new(
			timeSystem,
			TimeSpan.FromSeconds(10),
			_ =>
			{
				receivedIterations++;
				if (--cancelAfterIterations == 0)
				{
					cancellationTokenSource.Cancel();
				}
			});

		sut.Start(cancellationToken);

		cancellationToken.WaitHandle.WaitOne();
		receivedIterations.Should().BeGreaterOrEqualTo(cancelAfterIterations);
	}
}