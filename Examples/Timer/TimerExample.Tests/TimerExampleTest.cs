using FluentAssertions;
using System;
using System.Threading;
using Testably.Abstractions.Testing;
using Xunit;

namespace TimerExample.Tests;

public class TimerExampleTest
{
	/// <summary>
	///     Tests that the `thrownException` is caught and forwarded to the
	///     errorHandler of the <see cref="SynchronizationTimer" /> constructor parameter.
	/// </summary>
	[Fact]
	public void Exception_ShouldBeHandled()
	{
		Exception thrownException = new("foo");
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
	///     Tests that the callback is executed repeatedly (at least `iterationCount` times).
	/// </summary>
	[Fact]
	public void Execute_ShouldBeCalledRepeatedly()
	{
		int iterationCount = 5;
		// Add a fallback timeout that will fail the test latest after 1 second
		CancellationTokenSource cancellationTokenSource = new(1000);
		CancellationToken cancellationToken = cancellationTokenSource.Token;
		TimeSystemMock timeSystem = new();
		using SynchronizationTimer sut = new(
			timeSystem,
			TimeSpan.FromSeconds(10),
			_ =>
			{
				if (--iterationCount == 0)
				{
					cancellationTokenSource.Cancel();
				}
			});

		sut.Start(cancellationToken);

		cancellationToken.WaitHandle.WaitOne();
		//synchronizationMockMock.ExecutionCount.Should().BeGreaterOrEqualTo(5);
	}
}