#if NET6_0_OR_GREATER
#endif

namespace Testably.Abstractions.Testing.Tests;

public sealed class FileSystemInitializerOptionsTests
{
	[Fact]
	public void InitializeTempDirectory_ShouldBeInitializedToTrue()
	{
		FileSystemInitializerOptions sut = new();

		sut.InitializeTempDirectory.Should().BeTrue();
	}
}
