using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

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

		fileSystem.Statistics.TotalCount.Should().Be(0);
		foreach (DirectoryDescription directory in directories)
		{
			fileSystem.Directory.Exists(directory.Name).Should().BeTrue();
		}
	}

	[Theory]
	[AutoData]
	public void With_DirectoryDescriptions_WithSubdirectories_ShouldCreateDirectories(
		string parent, DirectoryDescription[] directories)
	{
		DirectoryDescription directoryDescription = new(parent,
			directories.Cast<FileSystemInfoDescription>().ToArray());
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(directoryDescription);

		fileSystem.Statistics.TotalCount.Should().Be(0);
		foreach (DirectoryDescription directory in directories)
		{
			fileSystem.Directory.Exists(Path.Combine(parent, directory.Name)).Should().BeTrue();
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

		fileSystem.Statistics.TotalCount.Should().Be(0);
		fileSystem.File.Exists(name).Should().BeTrue();
		fileSystem.File.ReadAllBytes(name).Should().BeEquivalentTo(bytes);
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

		fileSystem.Statistics.TotalCount.Should().Be(0);
		fileSystem.File.Exists(name).Should().BeTrue();
		fileSystem.File.ReadAllText(name).Should().BeEquivalentTo(content);
	}

	[Theory]
	[AutoData]
	public void With_FileDescriptions_ShouldCreateFiles(FileDescription[] files)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(files);

		fileSystem.Statistics.TotalCount.Should().Be(0);
		foreach (FileDescription file in files)
		{
			fileSystem.File.Exists(file.Name).Should().BeTrue();
		}
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public void With_FileDescriptions_ShouldSetIsReadOnlyFlag(bool isReadOnly, string name)
	{
		FileDescription description = new(name)
		{
			IsReadOnly = isReadOnly,
		};
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(description);

		fileSystem.Statistics.TotalCount.Should().Be(0);
		fileSystem.File.Exists(name).Should().BeTrue();
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

		fileSystem.Statistics.TotalCount.Should().Be(0);
		fileSystem.File.Exists(fileName).Should().BeTrue();
		fileSystem.Directory.Exists(directoryName).Should().BeTrue();
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

		fileSystem.Statistics.TotalCount.Should().Be(0);
		fileSystem.File.Exists(path).Should().BeTrue();
		fileSystem.File.ReadAllText(path).Should().BeEquivalentTo("foo");
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

		fileSystem.Statistics.TotalCount.Should().Be(0);
		fileSystem.File.Exists(path).Should().BeTrue();
		fileSystem.Directory.Exists(directoryPath).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void WithSubdirectories_ShouldCreateAllDirectories(string[] paths)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		IFileSystemInitializer<MockFileSystem> result = sut
			.WithSubdirectories(paths);

		fileSystem.Statistics.TotalCount.Should().Be(0);
		foreach (string path in paths)
		{
			fileSystem.Directory.Exists(path).Should().BeTrue();
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

		fileSystem.Statistics.TotalCount.Should().Be(0);
		fileSystem.Directory.Exists(path).Should().BeTrue();
		result.FileSystem.Should().BeSameAs(fileSystem);
	}
}
