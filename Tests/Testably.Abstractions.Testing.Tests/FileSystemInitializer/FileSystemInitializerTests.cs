using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class FileSystemInitializerTests
{
	[Theory]
	[AutoData]
	public async Task With_DirectoryDescriptions_ShouldCreateDirectories(
		DirectoryDescription[] directories)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(directories);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		foreach (DirectoryDescription directory in directories)
		{
			await That(fileSystem.Directory.Exists(directory.Name)).IsTrue();
		}
	}

	[Theory]
	[AutoData]
	public async Task With_DirectoryDescriptions_WithSubdirectories_ShouldCreateDirectories(
		string parent, DirectoryDescription[] directories)
	{
		DirectoryDescription directoryDescription = new(parent,
			directories.Cast<FileSystemInfoDescription>().ToArray());
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(directoryDescription);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		foreach (DirectoryDescription directory in directories)
		{
			await That(fileSystem.Directory.Exists(Path.Combine(parent, directory.Name))).IsTrue();
		}
	}

	[Theory]
	[AutoData]
	public async Task With_FileDescription_WithBytes_ShouldCreateFileContent(string name,
		byte[] bytes)
	{
		FileDescription description = new(name, bytes);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(description);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		await That(fileSystem.File.Exists(name)).IsTrue();
		await That(fileSystem.File.ReadAllBytes(name)).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task With_FileDescription_WithContent_ShouldCreateFileContent(string name,
		string content)
	{
		FileDescription description = new(name, content);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(description);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		await That(fileSystem.File.Exists(name)).IsTrue();
		await That(fileSystem.File.ReadAllText(name)).IsEqualTo(content);
	}

	[Theory]
	[AutoData]
	public async Task With_FileDescriptions_ShouldCreateFiles(FileDescription[] files)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(files);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		foreach (FileDescription file in files)
		{
			await That(fileSystem.File.Exists(file.Name)).IsTrue();
		}
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public async Task With_FileDescriptions_ShouldSetIsReadOnlyFlag(bool isReadOnly, string name)
	{
		FileDescription description = new(name)
		{
			IsReadOnly = isReadOnly,
		};
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(description);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		await That(fileSystem.File.Exists(name)).IsTrue();
		await That(fileSystem.FileInfo.New(name).IsReadOnly).IsEqualTo(isReadOnly);
	}

	[Theory]
	[AutoData]
	public async Task With_FilesAndDirectories_ShouldBothBeCreated(string fileName,
		string directoryName)
	{
		FileDescription fileDescription = new(fileName);
		DirectoryDescription directoryDescription = new(directoryName);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.With(fileDescription, directoryDescription);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		await That(fileSystem.File.Exists(fileName)).IsTrue();
		await That(fileSystem.Directory.Exists(directoryName)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task WithFile_ExistingDirectory_ShouldThrowTestingException(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();
		fileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			sut.WithFile(path);
		}

		await That(Act).ThrowsExactly<TestingException>().WithMessage($"*{path}*").AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task WithFile_ExistingFile_ShouldThrowTestingException(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();
		fileSystem.File.WriteAllText(path, "");

		void Act()
			=> sut.WithFile(path);

		await That(Act).ThrowsExactly<TestingException>().WithMessage($"*{path}*").AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task WithFile_HasStringContent_ShouldWriteFileContent(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.WithFile(path).Which(f => f.HasStringContent("foo"));

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		await That(fileSystem.File.Exists(path)).IsTrue();
		await That(fileSystem.File.ReadAllText(path)).IsEqualTo("foo");
	}

	[Theory]
	[AutoData]
	public async Task WithFile_MissingDirectory_ShouldCreateDirectory(string directoryPath,
		string fileName)
	{
		string path = Path.Combine(directoryPath, fileName);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		sut.WithFile(path);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		await That(fileSystem.File.Exists(path)).IsTrue();
		await That(fileSystem.Directory.Exists(directoryPath)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task WithSubdirectories_ShouldCreateAllDirectories(string[] paths)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		IFileSystemInitializer<MockFileSystem> result = sut
			.WithSubdirectories(paths);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		foreach (string path in paths)
		{
			await That(fileSystem.Directory.Exists(path)).IsTrue();
		}

		await That(result).IsEqualTo(sut);
	}

	[Theory]
	[AutoData]
	public async Task WithSubdirectory_ExistingDirectory_ShouldThrowTestingException(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();
		fileSystem.Directory.CreateDirectory(path);

		void Act()
			=> sut.WithSubdirectory(path);

		await That(Act).ThrowsExactly<TestingException>().WithMessage($"*{path}*").AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task WithSubdirectory_ExistingFile_ShouldThrowTestingException(string path)
	{
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();
		fileSystem.File.WriteAllBytes(path, Array.Empty<byte>());

		void Act()
			=> sut.WithSubdirectory(path);

		await That(Act).ThrowsExactly<TestingException>().WithMessage($"*{path}*").AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task WithSubdirectory_MultipleDirectoryLevels(string level1, string level2)
	{
		string path = Path.Combine(level1, level2);
		MockFileSystem fileSystem = new();
		IFileSystemInitializer<MockFileSystem> sut = fileSystem.Initialize();

		IFileSystemDirectoryInitializer<MockFileSystem> result = sut
			.WithSubdirectory(path);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		await That(fileSystem.Directory.Exists(path)).IsTrue();
		await That(result.FileSystem).IsSameAs(fileSystem);
	}
}
