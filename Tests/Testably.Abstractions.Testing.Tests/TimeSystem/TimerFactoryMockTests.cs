using System.Threading;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerFactoryMockTests
{
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
