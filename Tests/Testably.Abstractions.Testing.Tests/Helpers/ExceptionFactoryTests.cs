using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class ExceptionFactoryTests
{
	[Fact]
	public async Task NotSupportedSafeFileHandle_ShouldMentionWithSafeFileHandleStrategy()
	{
		NotSupportedException sut = ExceptionFactory.NotSupportedSafeFileHandle();

		await That(sut.Message).Contains(nameof(MockFileSystem.WithSafeFileHandleStrategy));
	}

	[Fact]
	public async Task OperationNotSupportedOnThisPlatform_ShouldMentionPlatform()
	{
		PlatformNotSupportedException sut = ExceptionFactory.OperationNotSupportedOnThisPlatform();

		await That(sut.Message).Contains("platform");
	}

	[Theory]
	[InlineAutoData(SimulationMode.Windows, false)]
	[InlineAutoData(SimulationMode.Windows, true)]
	public async Task PathCannotBeEmpty_ShouldSetParamNameExceptOnNetFramework(
		SimulationMode type, bool isNetFramework, string paramName)
	{
		#pragma warning disable CS0618
		Execute execute = new(new MockFileSystem(), type, isNetFramework);
		#pragma warning restore CS0618
		ArgumentException sut = ExceptionFactory.PathCannotBeEmpty(execute, paramName);

		await That(sut.Message).Contains("Path cannot be the empty string or all whitespace");
		await That(sut.HResult).IsEqualTo(-2147024809);
		if (isNetFramework)
		{
			await That(sut.ParamName).IsNull();
		}
		else
		{
			await That(sut.ParamName).IsEqualTo(paramName);
		}
	}

	[Fact]
	public async Task SearchPatternCannotContainTwoDots_ShouldMentionTwoDots()
	{
		ArgumentException sut = ExceptionFactory.SearchPatternCannotContainTwoDots();

		await That(sut.Message).Contains("\"..\"");
	}
}
