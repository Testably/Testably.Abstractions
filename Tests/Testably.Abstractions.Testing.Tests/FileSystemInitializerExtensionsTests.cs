using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		sut.Initialize().WithASubdirectory();

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
		sut.Initialize()
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

	[SkippableTheory]
	[AutoData]
	public void InitializeIn_MissingDrive_ShouldCreateDrive(string directoryName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		directoryName = Path.Combine("D:\\", directoryName);
		MockFileSystem sut = new();
		sut.InitializeIn(directoryName);

		sut.Directory.Exists(directoryName).Should().BeTrue();
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
