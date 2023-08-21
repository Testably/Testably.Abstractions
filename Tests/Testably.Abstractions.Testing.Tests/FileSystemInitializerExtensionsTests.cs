using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Testably.Abstractions.Testing.FileSystemInitializer;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests;

public class FileSystemInitializerExtensionsTests
{
	[Fact]
	public void Initialize_WithAFile_ShouldCreateFile()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithAFile();

		sut.Directory.EnumerateFiles(".").Should().ContainSingle();
	}

	[Theory]
	[AutoData]
	public void Initialize_WithAFile_WithExtension_ShouldCreateFileWithExtension(
		string extension)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithAFile(extension);

		sut.Directory.EnumerateFiles(".", $"*.{extension}").Should().ContainSingle();
	}

	[Fact]
	public void Initialize_WithASubdirectory_ShouldCreateDirectory()
	{
		MockFileSystem sut = new();
		sut.InitializeIn("base-directory").WithASubdirectory();

		sut.Directory.EnumerateDirectories(".").Should().ContainSingle();
	}

	[Theory]
	[AutoData]
	public void Initialize_WithFile_Existing_ShouldThrowTestingException(string fileName)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(fileName, null);
		Exception? exception = Record.Exception(() =>
		{
			sut.Initialize().WithFile(fileName);
		});

		exception.Should().BeOfType<TestingException>()
			.Which.Message.Should().Contain(fileName);
	}

	[Theory]
	[AutoData]
	public void Initialize_WithFile_HasBytesContent_ShouldCreateFileWithGivenFileContent(
		string fileName, byte[] fileContent)
	{
		MockFileSystem sut = new();
		sut.Initialize()
			.WithFile(fileName).Which(f => f
				.HasBytesContent(fileContent));

		byte[] result = sut.File.ReadAllBytes(fileName);

		result.Should().BeEquivalentTo(fileContent);
	}

	[Theory]
	[AutoData]
	public void Initialize_WithFile_HasStringContent_ShouldCreateFileWithGivenFileContent(
		string fileName, string fileContent)
	{
		MockFileSystem sut = new();
		sut.Initialize()
			.WithFile(fileName).Which(f => f
				.HasStringContent(fileContent));

		string result = sut.File.ReadAllText(fileName);

		result.Should().Be(fileContent);
	}

	[Theory]
	[AutoData]
	public void Initialize_WithFile_ShouldCreateFileWithGivenFileName(string fileName)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile(fileName);

		sut.Directory.EnumerateFiles(".", fileName).Should().ContainSingle();
	}

	[Fact]
	public void Initialize_WithNestedSubdirectories_ShouldCreateAllNestedDirectories()
	{
		MockFileSystem sut = new();
		sut.InitializeIn("base-directory")
			.WithSubdirectory("foo").Initialized(d => d
				.WithSubdirectory("bar").Initialized(s => s
					.WithSubdirectory("xyz")));

		List<string> result = sut.Directory
			.EnumerateDirectories(".", "*", SearchOption.AllDirectories).ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(sut.Path.Combine(".", "foo"));
		result.Should().Contain(sut.Path.Combine(".", "foo", "bar"));
		result.Should().Contain(sut.Path.Combine(".", "foo", "bar", "xyz"));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Initialize_WithOptions_ShouldConsiderValueOfInitializeTempDirectory(
		bool initializeTempDirectory)
	{
		MockFileSystem sut = new();

		sut.Initialize(options => options.InitializeTempDirectory = initializeTempDirectory);

		sut.Directory.Exists(sut.Path.GetTempPath()).Should().Be(initializeTempDirectory);
	}

	[Theory]
	[AutoData]
	public void Initialize_WithSubdirectory_Existing_ShouldThrowTestingException(
		string directoryName)
	{
		MockFileSystem sut = new();
		sut.Directory.CreateDirectory(directoryName);
		Exception? exception = Record.Exception(() =>
		{
			sut.Initialize().WithSubdirectory(directoryName);
		});

		exception.Should().BeOfType<TestingException>()
			.Which.Message.Should().Contain(directoryName);
	}

	[Theory]
	[AutoData]
	public void Initialize_WithSubdirectory_ShouldCreateDirectoryWithGivenDirectoryName(
		string directoryName)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory(directoryName);

		sut.Directory.EnumerateDirectories(".", directoryName).Should().ContainSingle();
	}

	[Theory]
	[AutoData]
	public void Initialize_WithSubdirectory_ShouldExist(string directoryName)
	{
		MockFileSystem sut = new();
		IFileSystemDirectoryInitializer<
			MockFileSystem> result =
			sut.Initialize().WithSubdirectory(directoryName);

		result.Directory.Exists.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void
		InitializeEmbeddedResourcesFromAssembly_ShouldCopyAllMatchingResourceFilesInDirectory(
			string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("foo");

		fileSystem.InitializeEmbeddedResourcesFromAssembly(
			path,
			Assembly.GetExecutingAssembly(),
			searchPattern: "*.txt");

		string[] result = fileSystem.Directory.GetFiles(Path.Combine(path, "TestResources"));
		string[] result2 =
			fileSystem.Directory.GetFiles(Path.Combine(path, "TestResources", "SubResource"));
		result.Length.Should().Be(2);
		result.Should().Contain(x => x.EndsWith("TestFile1.txt"));
		result.Should().Contain(x => x.EndsWith("TestFile2.txt"));
		result2.Length.Should().Be(1);
		result2.Should().Contain(x => x.EndsWith("SubResourceFile1.txt"));
	}

	[Theory]
	[AutoData]
	public void
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

		string[] result = fileSystem.Directory.GetFiles(path);
		result.Length.Should().Be(2);
		result.Should().Contain(x => x.EndsWith("TestFile1.txt"));
		result.Should().Contain(x => x.EndsWith("TestFile2.txt"));
		fileSystem.Should().NotHaveDirectory(Path.Combine(path, "SubResource"));
	}

	[Theory]
	[AutoData]
	public void
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

		string[] result = fileSystem.Directory.GetFiles(path);
		result.Length.Should().Be(1);
		result.Should().Contain(x => x.EndsWith("SubResourceFile1.txt"));
	}

	[SkippableTheory]
	[AutoData]
	public void InitializeIn_MissingDrive_ShouldCreateDrive(string directoryName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();
		IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		for (char c = 'D'; c <= 'Z'; c++)
		{
			if (drives.Any(d => d.Name.StartsWith($"{c}")))
			{
				continue;
			}

			directoryName = Path.Combine($"{c}:\\", directoryName);
			break;
		}

		sut.InitializeIn(directoryName);

		sut.Directory.Exists(directoryName).Should().BeTrue();
		sut.DriveInfo.GetDrives().Length.Should().Be(drives.Length + 1);
	}

	[Theory]
	[AutoData]
	public void InitializeIn_ShouldSetCurrentDirectory(string path)
	{
		MockFileSystem sut = new();
		string expectedPath = sut.Path.GetFullPath(path);

		sut.InitializeIn(path);

		sut.Directory.GetCurrentDirectory().Should().Be(expectedPath);
	}
}
