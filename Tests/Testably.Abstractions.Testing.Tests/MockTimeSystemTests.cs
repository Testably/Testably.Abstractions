using System.Threading;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests;

public class MockTimeSystemTests
{
	[Fact]
	public async Task Delay_Infinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(Timeout.Infinite, TestContext.Current.CancellationToken));

		await That(exception).IsNull();
	}

	[Fact]
	public async Task Delay_InfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(Timeout.InfiniteTimeSpan,
					TestContext.Current.CancellationToken));

		await That(exception).IsNull();
	}

	[Fact]
	public async Task Delay_LessThanInfinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();

		async Task Act()
			=> await timeSystem.Task.Delay(-2, TestContext.Current.CancellationToken);

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>()
			.WithParamName("millisecondsDelay");
	}

	[Fact]
	public async Task Delay_LessThanInfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();

		async Task Act()
			=> await timeSystem.Task.Delay(TimeSpan.FromMilliseconds(-2),
				TestContext.Current.CancellationToken);

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("delay");
	}

	[Theory]
	[InlineData(DateTimeKind.Local)]
	[InlineData(DateTimeKind.Unspecified)]
	[InlineData(DateTimeKind.Utc)]
	public async Task DifferenceBetweenDateTimeNowAndDateTimeUtcNow_ShouldBeLocalTimeZoneOffsetFromUtc(DateTimeKind dateTimeKind)
	{
		DateTime now = TimeTestHelper.GetRandomTime(DateTimeKind.Local);

		var expectedDifference = TimeZoneInfo.Local.GetUtcOffset(now);

		MockTimeSystem timeSystem = new(DateTime.SpecifyKind(now, dateTimeKind));
		var actualDifference = timeSystem.DateTime.Now - timeSystem.DateTime.UtcNow;

		await That(actualDifference).IsEqualTo(expectedDifference);
	}

	[Fact]
	public async Task Sleep_Infinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.Infinite));

		await That(exception).IsNull();
	}

	[Fact]
	public async Task Sleep_InfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.InfiniteTimeSpan));

		await That(exception).IsNull();
	}

	[Fact]
	public async Task Sleep_LessThanInfinite_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(-2));

		await That(exception).IsExactly<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async Task Sleep_LessThanInfiniteTimeSpan_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(()
				=> timeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

		await That(exception).IsExactly<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async Task ToString_WithFixedContainer_ShouldContainTimeProvider()
	{
		DateTime now = TimeTestHelper.GetRandomTime();
		MockTimeSystem timeSystem = new(TimeProvider.Use(now));

		string result = timeSystem.ToString();

		await That(result).Contains("Fixed");
		await That(result).Contains($"{now}Z");
	}

	[Fact]
	public async Task ToString_WithNowContainer_ShouldContainTimeProvider()
	{
		MockTimeSystem timeSystem = new(TimeProvider.Now());

		string result = timeSystem.ToString();

		await That(result).Contains("Now");
		await That(result).Contains($"{timeSystem.DateTime.UtcNow}Z");
	}

	[Fact]
	public async Task ToString_WithRandomContainer_ShouldContainTimeProvider()
	{
		MockTimeSystem timeSystem = new(TimeProvider.Random());

		string result = timeSystem.ToString();

		await That(result).Contains("Random");
		await That(result).Contains($"{timeSystem.DateTime.UtcNow}Z");
	}
}
