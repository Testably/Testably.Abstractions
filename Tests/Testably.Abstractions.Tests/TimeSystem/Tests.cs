namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	[Fact]
	public void DateTime_ShouldSetExtensionPoint()
	{
		ITimeSystem result = TimeSystem.DateTime.TimeSystem;

		result.Should().Be(TimeSystem);
	}

	[Fact]
	public void Task_ShouldSetExtensionPoint()
	{
		ITimeSystem result = TimeSystem.Task.TimeSystem;

		result.Should().Be(TimeSystem);
	}

	[Fact]
	public void Thread_ShouldSetExtensionPoint()
	{
		ITimeSystem result = TimeSystem.Thread.TimeSystem;

		result.Should().Be(TimeSystem);
	}
}