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

	[Theory]
	[InlineData("foo/bar/file.txt")]
	[InlineData(@"foo\bar\file.txt")]
	public void Verify_ShouldSupportPathWithSeparators(string path)
	{
		MockFileSystem sut = new MockFileSystem();
		sut.Directory.CreateDirectory(sut.Path.Combine("foo", "bar"));
		sut.File.WriteAllText(sut.Path.Combine("foo", "bar", "file.txt"), "some content");

		IFileSystemVerifier result = sut.Verify(path);

		result.Exists.Should().BeTrue();
	}
}
