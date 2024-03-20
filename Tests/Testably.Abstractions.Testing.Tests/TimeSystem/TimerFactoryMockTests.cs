using System.Threading;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerFactoryMockTests
{
	[Fact]
	public void New_WithoutPeriod_ShouldStillBeRegistered()
	{
		MockTimeSystem timeSystem = new();

		using ManualResetEventSlim ms = new();
		using ITimer timer = timeSystem.Timer.New(_ =>
		{
			ms.Set();
		});

		ms.Wait(300).Should().BeFalse();
		timeSystem.TimerHandler[0].Should().Be(timer);
	}

	[Fact]
	public void Wrap_ShouldThrowNotSupportedException()
	{
		MockTimeSystem timeSystem = new();

		Exception? exception = Record.Exception(() =>
		{
			timeSystem.Timer.Wrap(new Timer(_ => { }));
		});

		exception.Should().BeOfType<NotSupportedException>();
	}
}
