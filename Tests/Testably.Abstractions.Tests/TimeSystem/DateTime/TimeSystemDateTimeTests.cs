namespace Testably.Abstractions.Tests.TimeSystem.DateTime;

public abstract class TimeSystemDateTimeTests<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	#region Test Setup

	public TTimeSystem TimeSystem { get; }

	protected TimeSystemDateTimeTests(TTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	#endregion

	[Fact]
	[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.MaxValue))]
	public void MaxValue_ShouldReturnDefaultValue()
	{
		System.DateTime expectedResult = System.DateTime.MaxValue;

		System.DateTime result = TimeSystem.DateTime.MaxValue;

		result.Should().Be(expectedResult);
	}

	[Fact]
	[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.MinValue))]
	public void MinValue_ShouldReturnDefaultValue()
	{
		System.DateTime expectedResult = System.DateTime.MinValue;

		System.DateTime result = TimeSystem.DateTime.MinValue;

		result.Should().Be(expectedResult);
	}

	[Fact]
	[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.UnixEpoch))]
	public void UnixEpoch_ShouldReturnDefaultValue()
	{
		System.DateTime expectedResult = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		System.DateTime result = TimeSystem.DateTime.UnixEpoch;

		result.Should().Be(expectedResult);
	}
}