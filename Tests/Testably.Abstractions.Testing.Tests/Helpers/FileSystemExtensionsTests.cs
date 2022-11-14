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
}
