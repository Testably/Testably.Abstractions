using System.Collections.Generic;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerHandlerTests
{
	[Fact]
	public void Index_AccessDisposedIndex_ShouldThrowException()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using (timeSystem.Timer.New(_ => { }, null, 0, 100))
		{
		}

		Exception? exception = Record.Exception(() =>
		{
			_ = sut[0];
		});

		exception.Should().BeOfType<KeyNotFoundException>();
	}

	[Fact]
	public void Index_MultipleTimers_ShouldIncrement()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using ITimer timer0 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		using ITimer timer1 = timeSystem.Timer.New(_ => { }, null, 0, 100);

		sut[0].Should().Be(timer0);
		sut[1].Should().Be(timer1);
	}

	[Fact]
	public void Index_ShouldNotReuseDisposedIndexes()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using ITimer timer0 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		using ITimer timer1 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		// ReSharper disable once DisposeOnUsingVariable
		timer0.Dispose();
		using ITimer timer2 = timeSystem.Timer.New(_ => { }, null, 0, 100);

		sut[1].Should().Be(timer1);
		sut[2].Should().Be(timer2);
	}
}
