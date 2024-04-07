using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class FileSystemExtensionsTests
{
	[Theory]
	[AutoData]
	public void GetMoveLocation_LocationNotUnderSource_ShouldThrowNotSupportedException(
		string location, string source, string destination)
	{
		MockFileSystem sut = new();
		Exception? exception = Record.Exception(() =>
		{
			sut.GetMoveLocation(
				sut.Storage.GetLocation(location),
				sut.Storage.GetLocation(source),
				sut.Storage.GetLocation(destination));
		});

		exception.Should().BeOfType<NotSupportedException>().Which.Message
			.Should().Contain($"'{sut.Path.GetFullPath(location)}'")
			.And.Contain($"'{sut.Path.GetFullPath(source)}'");
	}

	[Fact]
	public void RandomOrDefault_WithMockFileSystem_ShouldUseRandomFromRandomSystem()
	{
		MockFileSystem fileSystem = new();
		IFileSystem sut = fileSystem;

		IRandom result = sut.RandomOrDefault();

		result.Should().Be(fileSystem.RandomSystem.Random.Shared);
	}

	[Fact]
	public void RandomOrDefault_WithRealFileSystem_ShouldUseSharedRandom()
	{
		RealFileSystem fileSystem = new();
		IFileSystem sut = fileSystem;

		IRandom result = sut.RandomOrDefault();

		result.Should().Be(RandomFactory.Shared);
	}
}
