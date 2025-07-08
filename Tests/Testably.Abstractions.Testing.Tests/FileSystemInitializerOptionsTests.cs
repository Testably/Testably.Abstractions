namespace Testably.Abstractions.Testing.Tests;

public sealed class FileSystemInitializerOptionsTests
{
	[Fact]
	public async Task InitializeTempDirectory_ShouldBeInitializedToTrue()
	{
		FileSystemInitializerOptions sut = new();

		await That(sut.InitializeTempDirectory).IsTrue();
	}
}
