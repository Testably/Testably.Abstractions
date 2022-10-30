namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DateTimeTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	[Fact]
	public void MaxValue_ShouldReturnDefaultValue()
	{
		System.DateTime expectedResult = System.DateTime.MaxValue;

		System.DateTime result = TimeSystem.DateTime.MaxValue;

		result.Should().Be(expectedResult);
	}

	[Fact]
	public void MinValue_ShouldReturnDefaultValue()
	{
		System.DateTime expectedResult = System.DateTime.MinValue;

		System.DateTime result = TimeSystem.DateTime.MinValue;

		result.Should().Be(expectedResult);
	}

	[Fact]
	public void UnixEpoch_ShouldReturnDefaultValue()
	{
		System.DateTime expectedResult = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		System.DateTime result = TimeSystem.DateTime.UnixEpoch;

		result.Should().Be(expectedResult);
	}
}