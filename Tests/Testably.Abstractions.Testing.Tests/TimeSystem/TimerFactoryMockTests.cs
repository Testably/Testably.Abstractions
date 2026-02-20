using System.Threading;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerFactoryMockTests
{
	[Fact]
	public async Task New_WithoutPeriod_ShouldStillBeRegistered()
	{
		MockTimeSystem timeSystem = new();

		using ManualResetEventSlim ms = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		});

		await That(ms.Wait(300, TestContext.Current.CancellationToken)).IsFalse();
		await That(timeSystem.TimerHandler[0]).IsEqualTo(timer);
	}

	[Fact]
	public async Task Wrap_ShouldThrowNotSupportedException()
	{
		MockTimeSystem timeSystem = new();

		void Act()
			=> timeSystem.Timer.Wrap(new Timer(_ => { }));

		await That(Act).ThrowsExactly<NotSupportedException>()
			.WithMessage("You cannot wrap an existing Timer in the MockTimeSystem instance!");
	}
}
