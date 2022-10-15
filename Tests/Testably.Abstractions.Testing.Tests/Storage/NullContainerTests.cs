using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class NullContainerTests
{
	[Fact]
	public void Constructor_ShouldSetFileAndTimeSystem()
	{
		Testing.FileSystemMock fileSystem = new();

		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.FileSystem.Should().Be(fileSystem);
		sut.TimeSystem.Should().Be(fileSystem.TimeSystem);
	}
}