using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.TimeSystem.DateTime;

public static class MockTimeSystemTests
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(Tests.TimeSystem.MockTimeSystemTests))]
	public sealed class DateTimeTests : TimeSystemDateTimeTests<TimeSystemMock>
	{
		#region Test Setup

		public DateTimeTests() : base(new TimeSystemMock())
		{
		}

		#endregion

		[Fact]
		public void MaxValue_FakedValue_ShouldReturnFakedValue()
		{
			System.DateTime fakedValue = TimeTestHelper.GetRandomTime();
			TimeSystem.TimeProvider.MaxValue = fakedValue;

			System.DateTime result = TimeSystem.DateTime.MaxValue;

			result.Should().Be(fakedValue);
		}

		[Fact]
		public void MinValue_FakedValue_ShouldReturnFakedValue()
		{
			System.DateTime fakedValue = TimeTestHelper.GetRandomTime();
			TimeSystem.TimeProvider.MinValue = fakedValue;

			System.DateTime result = TimeSystem.DateTime.MinValue;

			result.Should().Be(fakedValue);
		}

		[Fact]
		public void Now_Repeatedly_ShouldReturnSameTime()
		{
			System.DateTime result1 = TimeSystem.DateTime.Now;
			System.DateTime result2 = TimeSystem.DateTime.Now;

			result2.Should().Be(result1);
		}

		[Fact]
		public void Now_SetTime_ShouldReturnSetTime()
		{
			System.DateTime result1 = TimeSystem.DateTime.Now;
			System.DateTime setTime = result1.AddSeconds(10);

			TimeSystem.TimeProvider.SetTo(setTime);
			System.DateTime result2 = TimeSystem.DateTime.Now;

			result1.Should().NotBe(setTime);
			result2.Should().Be(setTime);
		}

		[Fact]
		public void Now_ShouldReturnLocalTime()
		{
			System.DateTime result = TimeSystem.DateTime.Now;

			result.Kind.Should().Be(DateTimeKind.Local);
		}

		[Fact]
		public void Today_Repeatedly_ShouldReturnSameTime()
		{
			System.DateTime result1 = TimeSystem.DateTime.Today;
			System.DateTime result2 = TimeSystem.DateTime.Today;

			result2.Should().Be(result1);
		}

		[Fact]
		public void Today_SetTime_ShouldReturnSetTime()
		{
			System.DateTime result1 = TimeSystem.DateTime.Today;
			System.DateTime setDate = result1.AddDays(10);

			TimeSystem.TimeProvider.SetTo(setDate);
			System.DateTime result2 = TimeSystem.DateTime.Today;

			result1.Should().NotBe(setDate);
			result2.Should().Be(setDate);
		}

		[Fact]
		public void Today_ShouldReturnDateWithoutTime()
		{
			System.DateTime result = TimeSystem.DateTime.Today;

			result.TimeOfDay.Should().Be(TimeSpan.Zero);
		}

		[Fact]
		public void UnixEpoch_FakedValue_ShouldReturnFakedValue()
		{
			System.DateTime fakedValue = TimeTestHelper.GetRandomTime();
			TimeSystem.TimeProvider.UnixEpoch = fakedValue;

			System.DateTime result = TimeSystem.DateTime.UnixEpoch;

			result.Should().Be(fakedValue);
		}

		[Fact]
		public void UtcNow_Repeatedly_ShouldReturnSameTime()
		{
			System.DateTime result1 = TimeSystem.DateTime.UtcNow;
			System.DateTime result2 = TimeSystem.DateTime.UtcNow;

			result2.Should().Be(result1);
		}

		[Fact]
		public void UtcNow_SetTime_ShouldReturnSetTime()
		{
			System.DateTime result1 = TimeSystem.DateTime.UtcNow;
			System.DateTime setTime = result1.AddSeconds(10);

			TimeSystem.TimeProvider.SetTo(setTime);
			System.DateTime result2 = TimeSystem.DateTime.UtcNow;

			result1.Should().NotBe(setTime);
			result2.Should().Be(setTime);
		}

		[Fact]
		public void UtcNow_ShouldReturnUniversalTime()
		{
			System.DateTime result = TimeSystem.DateTime.UtcNow;

			result.Kind.Should().Be(DateTimeKind.Utc);
		}
	}
}