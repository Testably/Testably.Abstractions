using System.IO;
using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class FileSystemInitializerTests
{
	[Theory]
	[AutoData]
	public void With_DirectoryDescriptions_ShouldCreateDirectories(
		DirectoryDescription[] directories)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(directories);

		foreach (DirectoryDescription directory in directories)
		{
			fileSystem.Should().HaveDirectory(directory.Name);
		}
	}

	[Theory]
	[AutoData]
	public void With_FileDescription_WithBytes_ShouldCreateFileContent(string name, byte[] bytes)
	{
		FileDescription description = new(name, bytes);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(description);

		fileSystem.Should().HaveFile(name)
			.Which.HasContent(bytes);
	}

	[Theory]
	[AutoData]
	public void With_FileDescription_WithContent_ShouldCreateFileContent(string name,
		string content)
	{
		FileDescription description = new(name, content);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(description);

		fileSystem.Should().HaveFile(name)
			.Which.HasContent(content);
	}

	[Theory]
	[AutoData]
	public void With_FileDescriptions_ShouldCreateFiles(FileDescription[] files)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(files);

		foreach (FileDescription file in files)
		{
			fileSystem.Should().HaveFile(file.Name);
		}
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public void With_FileDescriptions_ShouldSetIsReadOnlyFlag(bool isReadOnly, string name)
	{
		FileDescription description = new(name)
		{
			IsReadOnly = isReadOnly
		};
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(description);

		fileSystem.Should().HaveFile(name);
		fileSystem.FileInfo.New(name).IsReadOnly.Should().Be(isReadOnly);
	}

	[Theory]
	[AutoData]
	public void With_FilesAndDirectories_ShouldBothBeCreated(string fileName, string directoryName)
	{
		FileDescription fileDescription = new(fileName);
		DirectoryDescription directoryDescription = new(directoryName);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(fileDescription, directoryDescription);

		fileSystem.Should().HaveFile(fileName);
		fileSystem.Should().HaveDirectory(directoryName);
	}

	[Theory]
	[AutoData]
	public void WithFile_ExistingDirectory_ShouldThrowTestingException(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();
		fileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			sut.WithFile(path);
		});

		exception.Should().BeOfType<TestingException>()
			.Which.Message.Should().Contain(path);
	}

	[Theory]
	[AutoData]
	public void WithFile_ExistingFile_ShouldThrowTestingException(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();
		fileSystem.File.WriteAllText(path, "");

		Exception? exception = Record.Exception(() =>
			sut.WithFile(path));

		exception.Should().BeOfType<TestingException>()
			.Which.Message.Should().Contain(path);
	}

	[Theory]
	[AutoData]
	public void WithFile_HasStringContent_ShouldWriteFileContent(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.WithFile(path).Which(f => f.HasStringContent("foo"));

		fileSystem.Should().HaveFile(path)
			.Which.HasContent("foo");
	}

	[Theory]
	[AutoData]
	public void WithFile_MissingDirectory_ShouldCreateDirectory(string directoryPath,
		string fileName)
	{
		string path = Path.Combine(directoryPath, fileName);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.WithFile(path);

		fileSystem.Should().HaveFile(path);
		fileSystem.Should().HaveDirectory(directoryPath);
	}

	[Theory]
	[AutoData]
	public void WithSubdirectories_ShouldCreateAllDirectories(string[] paths)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		IFileSystemInitializer<MockFileSystem> result = sut
			.WithSubdirectories(paths);

		foreach (string path in paths)
		{
			fileSystem.Should().HaveDirectory(path);
		}

		result.Should().Be(sut);
	}

	[Theory]
	[AutoData]
	public void WithSubdirectory_ExistingDirectory_ShouldThrowTestingException(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();
		fileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
			sut.WithSubdirectory(path));

		exception.Should().BeOfType<TestingException>()
			.Which.Message.Should().Contain(path);
	}

	[Theory]
	[AutoData]
	public void WithSubdirectory_ExistingFile_ShouldThrowTestingException(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();
		fileSystem.File.WriteAllBytes(path, Array.Empty<byte>());

		Exception? exception = Record.Exception(() =>
			sut.WithSubdirectory(path));

		exception.Should().BeOfType<TestingException>()
			.Which.Message.Should().Contain(path);
	}

	[Theory]
	[AutoData]
	public void WithSubdirectory_MultipleDirectoryLevels(string level1, string level2)
	{
		string path = Path.Combine(level1, level2);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		IFileSystemDirectoryInitializer<MockFileSystem> result = sut
			.WithSubdirectory(path);

		fileSystem.Should().HaveDirectory(path);
		result.FileSystem.Should().BeSameAs(fileSystem);
	}
}
