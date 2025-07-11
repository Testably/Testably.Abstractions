using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimeSystemExtensibilityTests
{
	#region Test Setup

	public static TheoryData<ITimeSystem> GetTimeSystems
		=> new()
		{
			(ITimeSystem)new RealTimeSystem(),
			(ITimeSystem)new MockTimeSystem(),
		};

	#endregion

	[Theory]
	[MemberData(nameof(GetTimeSystems))]
	public async Task DateTime_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		IDateTime sut = timeSystem.DateTime;

		ITimeSystem result = sut.TimeSystem;

		await That(result).IsEqualTo(timeSystem);
	}

	[Theory]
	[MemberData(nameof(GetTimeSystems))]
	public async Task Task_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		ITask sut = timeSystem.Task;

		ITimeSystem result = sut.TimeSystem;

		await That(result).IsEqualTo(timeSystem);
	}

	[Theory]
	[MemberData(nameof(GetTimeSystems))]
	public async Task Thread_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		IThread sut = timeSystem.Thread;

		ITimeSystem result = sut.TimeSystem;

		await That(result).IsEqualTo(timeSystem);
	}

	[Theory]
	[MemberData(nameof(GetTimeSystems))]
	public async Task Timer_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		using ITimer sut = timeSystem.Timer.New(_ => { });

		ITimeSystem result = sut.TimeSystem;

		await That(result).IsEqualTo(timeSystem);
	}

	[Theory]
	[MemberData(nameof(GetTimeSystems))]
	public async Task TimerFactory_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		ITimerFactory sut = timeSystem.Timer;

		ITimeSystem result = sut.TimeSystem;

		await That(result).IsEqualTo(timeSystem);
	}
}
