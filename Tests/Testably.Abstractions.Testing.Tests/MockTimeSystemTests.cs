using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests;

public class MockTimeSystemTests
{
	[Fact]
	public async Task Delay_Infinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(() => timeSystem.Task.Delay(Timeout.Infinite));

		exception.Should().BeNull();
	}

	[Fact]
	public async Task Delay_InfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(Timeout.InfiniteTimeSpan));

		exception.Should().BeNull();
	}

	[Fact]
	public async Task Delay_LessThanInfinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(() => timeSystem.Task.Delay(-2));

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("millisecondsDelay");
	}

	[Fact]
	public async Task Delay_LessThanInfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			await Record.ExceptionAsync(()
				=> timeSystem.Task.Delay(TimeSpan.FromMilliseconds(-2)));

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("delay");
	}

	[Fact]
	public void Sleep_Infinite_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.Infinite));

		exception.Should().BeNull();
	}

	[Fact]
	public void Sleep_InfiniteTimeSpan_ShouldNotThrowException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(Timeout.InfiniteTimeSpan));

		exception.Should().BeNull();
	}

	[Fact]
	public void Sleep_LessThanInfinite_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(() => timeSystem.Thread.Sleep(-2));

		exception.Should().BeOfType<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Sleep_LessThanInfiniteTimeSpan_ShouldThrowArgumentOutOfRangeException()
	{
		MockTimeSystem timeSystem = new();
		Exception? exception =
			Record.Exception(()
				=> timeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

		exception.Should().BeOfType<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void ToString_WithFixedContainer_ShouldContainTimeProvider()
	{
		DateTime now = TimeTestHelper.GetRandomTime();
		MockTimeSystem timeSystem = new(TimeProvider.Use(now));

		string result = timeSystem.ToString();

		result.Should().Contain("Fixed");
		result.Should().Contain($"{now.ToUniversalTime()}Z");
	}

	[Fact]
	public void ToString_WithNowContainer_ShouldContainTimeProvider()
	{
		MockTimeSystem timeSystem = new(TimeProvider.Now());

		string result = timeSystem.ToString();

		result.Should().Contain("Now");
		result.Should().Contain($"{timeSystem.DateTime.UtcNow}Z");
	}

	[Fact]
	public void ToString_WithRandomContainer_ShouldContainTimeProvider()
	{
		MockTimeSystem timeSystem = new(TimeProvider.Random());

		string result = timeSystem.ToString();

		result.Should().Contain("Random");
		result.Should().Contain($"{timeSystem.DateTime.UtcNow}Z");
	}
}
