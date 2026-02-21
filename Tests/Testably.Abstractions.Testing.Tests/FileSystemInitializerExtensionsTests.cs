using aweXpect.Testably;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests;

public class FileSystemInitializerExtensionsTests
{
	[Fact]
	public async Task Initialize_WithAFile_ShouldCreateFile()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithAFile();

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateFiles(".")).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task Initialize_WithAFile_WithExtension_ShouldCreateFileWithExtension(
		string extension)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithAFile(extension);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateFiles(".", $"*.{extension}")).HasSingle();
	}

	[Fact]
	public async Task Initialize_WithASubdirectory_ShouldCreateDirectory()
	{
		MockFileSystem sut = new();
		sut.InitializeIn("base-directory").WithASubdirectory();

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateDirectories(".")).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task Initialize_WithFile_Existing_ShouldThrowTestingException(string fileName)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(fileName, null);

		void Act()
		{
			sut.Initialize().WithFile(fileName);
		}

		await That(Act).ThrowsExactly<TestingException>().WithMessage("*fileName*").AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task Initialize_WithFile_HasBytesContent_ShouldCreateFileWithGivenFileContent(
		string fileName, byte[] fileContent)
	{
		MockFileSystem sut = new();
		sut.Initialize()
			.WithFile(fileName).Which(f => f
				.HasBytesContent(fileContent));

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		byte[] result = sut.File.ReadAllBytes(fileName);

		await That(result).IsEqualTo(fileContent).InAnyOrder();
	}

	[Theory]
	[AutoData]
	public async Task Initialize_WithFile_HasStringContent_ShouldCreateFileWithGivenFileContent(
		string fileName, string fileContent)
	{
		MockFileSystem sut = new();
		sut.Initialize()
			.WithFile(fileName).Which(f => f
				.HasStringContent(fileContent));

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		string result = sut.File.ReadAllText(fileName);

		await That(result).IsEqualTo(fileContent);
	}

	[Theory]
	[AutoData]
	public async Task Initialize_WithFile_ShouldCreateFileWithGivenFileName(string fileName)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile(fileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateFiles(".", fileName)).HasSingle();
	}

	[Fact]
	public async Task Initialize_WithNestedSubdirectories_ShouldCreateAllNestedDirectories()
	{
		MockFileSystem sut = new();
		sut.InitializeIn("base-directory")
			.WithSubdirectory("foo").Initialized(d => d
				.WithSubdirectory("bar").Initialized(s => s
					.WithSubdirectory("xyz")));

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		List<string> result = sut.Directory
			.EnumerateDirectories(".", "*", SearchOption.AllDirectories).ToList();

		await That(result.Count).IsEqualTo(3);
		await That(result).Contains(sut.Path.Combine(".", "foo"));
		await That(result).Contains(sut.Path.Combine(".", "foo", "bar"));
		await That(result).Contains(sut.Path.Combine(".", "foo", "bar", "xyz"));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Initialize_WithOptions_ShouldConsiderValueOfInitializeTempDirectory(
		bool initializeTempDirectory)
	{
		MockFileSystem sut = new();

		sut.Initialize(options => options.InitializeTempDirectory = initializeTempDirectory);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.Exists(sut.Path.GetTempPath())).IsEqualTo(initializeTempDirectory);
	}

	[Theory]
	[AutoData]
	public async Task Initialize_WithSubdirectory_Existing_ShouldThrowTestingException(
		string directoryName)
	{
		MockFileSystem sut = new();
		sut.Directory.CreateDirectory(directoryName);

		void Act()
			=> sut.Initialize().WithSubdirectory(directoryName);

		await That(Act).ThrowsExactly<TestingException>().WithMessage($"*{directoryName}*")
			.AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task Initialize_WithSubdirectory_ShouldCreateDirectoryWithGivenDirectoryName(
		string directoryName)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory(directoryName);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateDirectories(".", directoryName)).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task Initialize_WithSubdirectory_ShouldExist(string directoryName)
	{
		MockFileSystem sut = new();
		IFileSystemDirectoryInitializer<
			MockFileSystem> result =
			sut.Initialize().WithSubdirectory(directoryName);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(result.Directory.Exists).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task
		InitializeEmbeddedResourcesFromAssembly_ShouldCopyAllMatchingResourceFilesInDirectory(
			string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("foo");

		fileSystem.InitializeEmbeddedResourcesFromAssembly(
			path,
			Assembly.GetExecutingAssembly(),
			searchPattern: "*.txt");

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		string[] result = fileSystem.Directory.GetFiles(Path.Combine(path, "TestResources"));
		string[] result2 =
			fileSystem.Directory.GetFiles(Path.Combine(path, "TestResources", "SubResource"));
		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(x => x.EndsWith("TestFile1.txt", StringComparison.Ordinal));
		await That(result).Contains(x => x.EndsWith("TestFile2.txt", StringComparison.Ordinal));
		await That(result2.Length).IsEqualTo(1);
		await That(result2)
			.Contains(x => x.EndsWith("SubResourceFile1.txt", StringComparison.Ordinal));
	}

	[Theory]
	[AutoData]
	public async Task
		InitializeEmbeddedResourcesFromAssembly_WithoutRecurseSubdirectories_ShouldOnlyCopyTopmostFilesInRelativePath(
			string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("foo");

		fileSystem.InitializeEmbeddedResourcesFromAssembly(
			path,
			Assembly.GetExecutingAssembly(),
			"TestResources",
			searchPattern: "*.txt",
			SearchOption.TopDirectoryOnly);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		string[] result = fileSystem.Directory.GetFiles(path);
		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(x => x.EndsWith("TestFile1.txt", StringComparison.Ordinal));
		await That(result).Contains(x => x.EndsWith("TestFile2.txt", StringComparison.Ordinal));
		await That(fileSystem.Directory.Exists(Path.Combine(path, "SubResource"))).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task
		InitializeEmbeddedResourcesFromAssembly_WithRelativePath_ShouldCopyAllResourceInMatchingPathInDirectory(
			string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("foo");

		fileSystem.InitializeEmbeddedResourcesFromAssembly(
			path,
			Assembly.GetExecutingAssembly(),
			"TestResources/SubResource",
			searchPattern: "*.txt");

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		string[] result = fileSystem.Directory.GetFiles(path);
		await That(result.Length).IsEqualTo(1);
		await That(result)
			.Contains(x => x.EndsWith("SubResourceFile1.txt", StringComparison.Ordinal));
	}

	[Theory]
	[AutoData]
	public async Task InitializeFromRealDirectory_MissingDrive_ShouldCreateDrive(
		string directoryName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			MockFileSystem sut = new();
			IDriveInfo[] drives = sut.DriveInfo.GetDrives();
			for (char c = 'D'; c <= 'Z'; c++)
			{
				if (drives.Any(d => d.Name.StartsWith($"{c}", StringComparison.Ordinal)))
				{
					continue;
				}

				directoryName = Path.Combine($"{c}:\\", directoryName);
				break;
			}

			sut.InitializeFromRealDirectory(tempPath, directoryName);

			await That(sut.Directory.Exists(directoryName)).IsTrue();
			await That(sut.DriveInfo.GetDrives()).HasCount(drives.Length + 1);
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Fact]
	public async Task InitializeFromRealDirectory_ShouldCopyFileToTargetDirectory()
	{
		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string filePath = Path.Combine(tempPath, "test.txt");
			File.WriteAllText(filePath, "Hello, World!");

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem).HasDirectory("foo");
			await That(fileSystem).HasFile("foo/test.txt").WithContent("Hello, World!");
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Fact]
	public async Task
		InitializeFromRealDirectory_ShouldRecursivelyCopyDirectoriesToTargetDirectory()
	{
		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			Directory.CreateDirectory(Path.Combine(tempPath, "subdir"));
			File.WriteAllText(Path.Combine(tempPath, "subdir", "test1.txt"), "foo");
			File.WriteAllText(Path.Combine(tempPath, "subdir", "test2.txt"), "bar");

			fileSystem.InitializeFromRealDirectory(tempPath);

			await That(fileSystem).HasDirectory(fileSystem.Path.Combine(tempPath, "subdir"));
			await That(fileSystem).HasFile(fileSystem.Path.Combine(tempPath, "subdir", "test1.txt"))
				.WithContent("foo");
			await That(fileSystem).HasFile(fileSystem.Path.Combine(tempPath, "subdir", "test2.txt"))
				.WithContent("bar");
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Fact]
	public async Task
		InitializeFromRealDirectory_WhenDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException()
	{
		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		while (Directory.Exists(tempPath))
		{
			tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		void Act()
			=> fileSystem.InitializeFromRealDirectory(tempPath, "foo");

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessage($"The directory '{tempPath}' does not exist.");
	}

	[Theory]
	[AutoData]
	public async Task InitializeIn_MissingDrive_ShouldCreateDrive(string directoryName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();
		IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		for (char c = 'D'; c <= 'Z'; c++)
		{
			if (drives.Any(d => d.Name.StartsWith($"{c}", StringComparison.Ordinal)))
			{
				continue;
			}

			directoryName = Path.Combine($"{c}:\\", directoryName);
			break;
		}

		sut.InitializeIn(directoryName);

		await That(sut.Directory.Exists(directoryName)).IsTrue();
		await That(sut.DriveInfo.GetDrives()).HasCount(drives.Length + 1);
	}

	[Theory]
	[AutoData]
	public async Task InitializeIn_ShouldSetCurrentDirectory(string path)
	{
		MockFileSystem sut = new();
		string expectedPath = sut.Execute.Path.GetFullPath(path);

		sut.InitializeIn(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.GetCurrentDirectory()).IsEqualTo(expectedPath);
	}
}
