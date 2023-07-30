using Testably.Abstractions.Testing.FileSystemVerifier;

namespace Testably.Abstractions.Testing.Tests.FileSystemVerifier;

public class FileSystemVerifierTests
{
	[Theory]
	[AutoData]
	public void Exists_WithDirectory_ShouldBeUpdated(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Directory.CreateDirectory(path);

		IFileSystemVerifier sut = fileSystem.Verify(path);

		sut.Exists.Should().BeTrue();
		fileSystem.Directory.Delete(path);
		sut.Exists.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Exists_WithFile_ShouldBeUpdated(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "foo");

		IFileSystemVerifier sut = fileSystem.Verify(path);

		sut.Exists.Should().BeTrue();
		fileSystem.File.Delete(path);
		sut.Exists.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Type_WithDirectory_ShouldBeSetToDirectory(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Directory.CreateDirectory(path);

		IFileSystemVerifier sut = fileSystem.Verify(path);

		sut.Type.Should().Be(FileSystemTypes.Directory);
	}

	[Theory]
	[AutoData]
	public void Type_WithFile_ShouldBeSetToFile(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "foo");

		IFileSystemVerifier sut = fileSystem.Verify(path);

		sut.Type.Should().Be(FileSystemTypes.File);
	}

	[Theory]
	[AutoData]
	public void Type_MissingPath_ShouldBeSetToDirectoryOrFile(string path)
	{
		MockFileSystem fileSystem = new();

		IFileSystemVerifier sut = fileSystem.Verify(path);

		sut.Type.Should().Be(FileSystemTypes.DirectoryOrFile);
	}
}
