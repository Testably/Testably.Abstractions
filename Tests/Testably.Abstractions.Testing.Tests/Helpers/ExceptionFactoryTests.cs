using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class ExceptionFactoryTests
{
	[Fact]
	public void NotSupportedSafeFileHandle_ShouldMentionWithSafeFileHandleStrategy()
	{
		NotSupportedException sut = ExceptionFactory.NotSupportedSafeFileHandle();

		sut.Message.Should().Contain(nameof(MockFileSystem.WithSafeFileHandleStrategy));
	}

	[Fact]
	public void OperationNotSupportedOnThisPlatform_ShouldMentionPlatform()
	{
		PlatformNotSupportedException sut = ExceptionFactory.OperationNotSupportedOnThisPlatform();

		sut.Message.Should().Contain("platform");
	}

	[Theory]
	[InlineAutoData(SimulationMode.Windows, false)]
	[InlineAutoData(SimulationMode.Windows, true)]
	public void PathCannotBeEmpty_ShouldSetParamNameExceptOnNetFramework(
		SimulationMode type, bool isNetFramework, string paramName)
	{
		#pragma warning disable CS0618
		Execute execute = new(new MockFileSystem(), type, isNetFramework);
		#pragma warning restore CS0618
		ArgumentException sut = ExceptionFactory.PathCannotBeEmpty(execute, paramName);

		sut.Message.Should().Contain("Path cannot be the empty string or all whitespace");
		sut.HResult.Should().Be(-2147024809);
		if (isNetFramework)
		{
			sut.ParamName.Should().BeNull();
		}
		else
		{
			sut.ParamName.Should().Be(paramName);
		}
	}

	[Fact]
	public void SearchPatternCannotContainTwoDots_ShouldMentionTwoDots()
	{
		ArgumentException sut = ExceptionFactory.SearchPatternCannotContainTwoDots();

		sut.Message.Should().Contain("\"..\"");
	}
}
