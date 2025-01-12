using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimeSystemExtensibilityTests
{
	#region Test Setup

	public static TheoryData<ITimeSystem> GetTimeSystems
		=> new()
		{
			new RealTimeSystem(),
			new MockTimeSystem(),
		};

	#endregion

	[SkippableTheory]
	[MemberData(nameof(GetTimeSystems))]
	public void DateTime_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		IDateTime sut = timeSystem.DateTime;

		ITimeSystem result = sut.TimeSystem;

		result.Should().Be(timeSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetTimeSystems))]
	public void Task_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		ITask sut = timeSystem.Task;

		ITimeSystem result = sut.TimeSystem;

		result.Should().Be(timeSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetTimeSystems))]
	public void Thread_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		IThread sut = timeSystem.Thread;

		ITimeSystem result = sut.TimeSystem;

		result.Should().Be(timeSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetTimeSystems))]
	public void Timer_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		using ITimer sut = timeSystem.Timer.New(_ => { });

		ITimeSystem result = sut.TimeSystem;

		result.Should().Be(timeSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetTimeSystems))]
	public void TimerFactory_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		ITimerFactory sut = timeSystem.Timer;

		ITimeSystem result = sut.TimeSystem;

		result.Should().Be(timeSystem);
	}
}
