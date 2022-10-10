using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockTimeSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(MockTimeSystem))]
	public sealed class DateTimeTests : TimeSystemDateTimeTests<TimeSystemMock>
	{
		#region Test Setup

		public DateTimeTests() : base(new TimeSystemMock())
		{
		}

		#endregion

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.MaxValue))]
		public void MaxValue_FakedValue_ShouldReturnFakedValue()
		{
			DateTime fakedValue = TimeTestHelper.GetRandomTime();
			TimeSystem.TimeProvider.MaxValue = fakedValue;

			DateTime result = TimeSystem.DateTime.MaxValue;

			result.Should().Be(fakedValue);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.MinValue))]
		public void MinValue_FakedValue_ShouldReturnFakedValue()
		{
			DateTime fakedValue = TimeTestHelper.GetRandomTime();
			TimeSystem.TimeProvider.MinValue = fakedValue;

			DateTime result = TimeSystem.DateTime.MinValue;

			result.Should().Be(fakedValue);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.Now))]
		public void Now_Repeatedly_ShouldReturnSameTime()
		{
			DateTime result1 = TimeSystem.DateTime.Now;
			DateTime result2 = TimeSystem.DateTime.Now;

			result2.Should().Be(result1);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.Now))]
		public void Now_SetTime_ShouldReturnSetTime()
		{
			DateTime result1 = TimeSystem.DateTime.Now;
			DateTime setTime = result1.AddSeconds(10);

			TimeSystem.TimeProvider.SetTo(setTime);
			DateTime result2 = TimeSystem.DateTime.Now;

			result1.Should().NotBe(setTime);
			result2.Should().Be(setTime);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.Now))]
		public void Now_ShouldReturnLocalTime()
		{
			DateTime result = TimeSystem.DateTime.Now;

			result.Kind.Should().Be(DateTimeKind.Local);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.Today))]
		public void Today_Repeatedly_ShouldReturnSameTime()
		{
			DateTime result1 = TimeSystem.DateTime.Today;
			DateTime result2 = TimeSystem.DateTime.Today;

			result2.Should().Be(result1);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.Today))]
		public void Today_SetTime_ShouldReturnSetTime()
		{
			DateTime result1 = TimeSystem.DateTime.Today;
			DateTime setDate = result1.AddDays(10);

			TimeSystem.TimeProvider.SetTo(setDate);
			DateTime result2 = TimeSystem.DateTime.Today;

			result1.Should().NotBe(setDate);
			result2.Should().Be(setDate);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.Today))]
		public void Today_ShouldReturnDateWithoutTime()
		{
			DateTime result = TimeSystem.DateTime.Today;

			result.TimeOfDay.Should().Be(TimeSpan.Zero);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.UnixEpoch))]
		public void UnixEpoch_FakedValue_ShouldReturnFakedValue()
		{
			DateTime fakedValue = TimeTestHelper.GetRandomTime();
			TimeSystem.TimeProvider.UnixEpoch = fakedValue;

			DateTime result = TimeSystem.DateTime.UnixEpoch;

			result.Should().Be(fakedValue);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.UtcNow))]
		public void UtcNow_Repeatedly_ShouldReturnSameTime()
		{
			DateTime result1 = TimeSystem.DateTime.UtcNow;
			DateTime result2 = TimeSystem.DateTime.UtcNow;

			result2.Should().Be(result1);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.UtcNow))]
		public void UtcNow_SetTime_ShouldReturnSetTime()
		{
			DateTime result1 = TimeSystem.DateTime.UtcNow;
			DateTime setTime = result1.AddSeconds(10);

			TimeSystem.TimeProvider.SetTo(setTime);
			DateTime result2 = TimeSystem.DateTime.UtcNow;

			result1.Should().NotBe(setTime);
			result2.Should().Be(setTime);
		}

		[Fact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.UtcNow))]
		public void UtcNow_ShouldReturnUniversalTime()
		{
			DateTime result = TimeSystem.DateTime.UtcNow;

			result.Kind.Should().Be(DateTimeKind.Utc);
		}
	}
}