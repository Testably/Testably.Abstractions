using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing.Tests.Internal;

public class FileSystemExtensionsTests
{
	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(FileSystemExtensions))]
	public void GetMoveLocation_LocationNotUnderSource_ShouldThrowNotSupportedException(
		string location, string source, string destination)
	{
		FileSystemMock sut = new();
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
}