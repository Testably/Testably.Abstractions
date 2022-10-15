using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.TimeSystem.DateTime;

public static partial class RealTimeSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(Tests.TimeSystem.RealTimeSystemTests))]
	public sealed class DateTimeTests : TimeSystemDateTimeTests<Abstractions.TimeSystem>
	{
		#region Test Setup

		public DateTimeTests() : base(new Abstractions.TimeSystem())
		{
		}

		#endregion

		[SkippableFact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.Now))]
		public void Now_ShouldReturnDefaultValue()
		{
			System.DateTime begin = System.DateTime.Now;

			System.DateTime result = TimeSystem.DateTime.Now;

			System.DateTime end = System.DateTime.Now;

			result.Kind.Should().Be(DateTimeKind.Local);
			result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
		}

		[SkippableFact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.Today))]
		public void Today_ShouldReturnDefaultValue()
		{
			System.DateTime begin = System.DateTime.Today;

			System.DateTime result = TimeSystem.DateTime.Today;

			System.DateTime end = System.DateTime.Today;

			result.Kind.Should().Be(begin.Kind);
			result.TimeOfDay.Should().Be(TimeSpan.Zero);
			result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
		}

		[SkippableFact]
		[TimeSystemTests.DateTime(nameof(ITimeSystem.IDateTime.UtcNow))]
		public void UtcNow_ShouldReturnDefaultValue()
		{
			System.DateTime begin = System.DateTime.UtcNow;

			System.DateTime result = TimeSystem.DateTime.UtcNow;

			System.DateTime end = System.DateTime.UtcNow;

			result.Kind.Should().Be(DateTimeKind.Utc);
			result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
		}
	}
}