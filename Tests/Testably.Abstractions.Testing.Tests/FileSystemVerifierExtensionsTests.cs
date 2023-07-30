using Testably.Abstractions.Testing.FileSystemVerifier;

namespace Testably.Abstractions.Testing.Tests;

public class FileSystemVerifierExtensionsTests
{
	[Theory]
	[AutoData]
	public void Verify_MissingFileOrDirectory_ShouldReturnNullObject(string path)
	{
		RealFileSystem sut = new();

		IFileSystemVerifier result = sut.Verify(path);

		result.Should().NotBeNull();
		result.Exists.Should().BeFalse();
	}
}
