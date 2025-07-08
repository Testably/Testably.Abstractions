using System.Collections.Generic;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimerHandlerTests
{
	[Fact]
	public async Task Index_AccessDisposedIndex_ShouldThrowException()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using (timeSystem.Timer.New(_ => { }, null, 0, 100))
		{
			// Immediately dispose the created timer.
		}

		Exception? exception = Record.Exception(() =>
		{
			_ = sut[0];
		});

		await That(exception).IsExactly<KeyNotFoundException>();
	}

	[Fact]
	public async Task Index_MultipleTimers_ShouldIncrement()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using ITimer timer0 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		using ITimer timer1 = timeSystem.Timer.New(_ => { }, null, 0, 100);

		await That(sut[0]).IsEqualTo(timer0);
		await That(sut[1]).IsEqualTo(timer1);
	}

	[Fact]
	public async Task Index_ShouldNotReuseDisposedIndexes()
	{
		MockTimeSystem timeSystem = new MockTimeSystem()
			.WithTimerStrategy(new TimerStrategy(TimerMode.StartOnMockWait));
		ITimerHandler sut = timeSystem.TimerHandler;

		using ITimer timer0 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		using ITimer timer1 = timeSystem.Timer.New(_ => { }, null, 0, 100);
		// ReSharper disable once DisposeOnUsingVariable
		timer0.Dispose();
		using ITimer timer2 = timeSystem.Timer.New(_ => { }, null, 0, 100);

		await That(sut[1]).IsEqualTo(timer1);
		await That(sut[2]).IsEqualTo(timer2);
	}
}
