using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class NullContainerTests
{
	[Fact]
	[Trait(nameof(Testing), nameof(NullContainer))]
	public void Constructor_ShouldSetFileAndTimeSystem()
	{
		FileSystemMock fileSystem = new();

		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.FileSystem.Should().Be(fileSystem);
		sut.TimeSystem.Should().Be(fileSystem.TimeSystem);
	}
}