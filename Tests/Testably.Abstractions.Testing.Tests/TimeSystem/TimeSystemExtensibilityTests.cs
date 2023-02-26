using System.Collections.Generic;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimeSystemExtensibilityTests
{
	[SkippableTheory]
	[MemberData(nameof(GetTimeSystems))]
	public void DateTime_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		ITimeSystem result = timeSystem.DateTime.TimeSystem;

		result.Should().Be(timeSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetTimeSystems))]
	public void Task_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		ITimeSystem result = timeSystem.Task.TimeSystem;

		result.Should().Be(timeSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetTimeSystems))]
	public void Thread_ShouldSetExtensionPoint(ITimeSystem timeSystem)
	{
		ITimeSystem result = timeSystem.Thread.TimeSystem;

		result.Should().Be(timeSystem);
	}

	public static IEnumerable<object[]> GetTimeSystems =>
		new List<object[]>
		{
			new object[]
			{
				new RealTimeSystem()
			},
			new object[]
			{
				new MockTimeSystem()
			},
		};
}
