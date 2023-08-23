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

	[Fact]
	public void SearchPatternCannotContainTwoDots_ShouldMentionTwoDots()
	{
		ArgumentException sut = ExceptionFactory.SearchPatternCannotContainTwoDots();

		sut.Message.Should().Contain("\"..\"");
	}
}
