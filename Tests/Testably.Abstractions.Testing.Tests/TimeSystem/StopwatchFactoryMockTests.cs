using System.Diagnostics;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class StopwatchFactoryMockTests
{
	[Fact]
	public async Task Wrap_ShouldThrowNotSupportedException()
	{
		MockTimeSystem timeSystem = new();

		void Act()
			=> timeSystem.Stopwatch.Wrap(new Stopwatch());

		await That(Act).ThrowsExactly<NotSupportedException>()
			.WithMessage("You cannot wrap an existing Stopwatch in the MockTimeSystem instance!");
	}
}
