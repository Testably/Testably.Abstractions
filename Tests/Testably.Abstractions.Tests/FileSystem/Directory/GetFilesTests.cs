using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
using System.Globalization;
#endif

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class GetFilesTests
{
	[Theory]
	[AutoData]
	public void
		GetFiles_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.GetFiles(path).ToList());

		exception.Should().BeException<DirectoryNotFoundException>(expectedPath,
			hResult: -2147024893);
		FileSystem.Directory.Exists(path).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void GetFiles_Path_NotOnLinux_ShouldBeCaseInsensitive(string path)
	{
		Skip.If(Test.RunsOnLinux);

		FileSystem.Initialize()
			.WithSubdirectory(path.ToUpperInvariant()).Initialized(s => s
				.WithAFile());

		string[] result = FileSystem.Directory.GetFiles(path.ToLowerInvariant());

		result.Length.Should().Be(1);
	}

	[Theory]
	[AutoData]
	public void GetFiles_Path_OnLinux_ShouldBeCaseSensitive(string path)
	{
		Skip.IfNot(Test.RunsOnLinux);

		FileSystem.Initialize()
			.WithSubdirectory(path.ToUpperInvariant()).Initialized(s => s
				.WithAFile());

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.GetFiles(path.ToLowerInvariant());
		});
		string[] result2 = FileSystem.Directory.GetFiles(path.ToUpperInvariant());

		exception.Should().BeOfType<DirectoryNotFoundException>();
		result2.Length.Should().Be(1);
	}

	[Theory]
	[AutoData]
	public void
		GetFiles_SearchOptionAllDirectories_FullPath_ShouldReturnAllFilesWithFullPath(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(FileSystem.Directory.GetCurrentDirectory(),
				"*", SearchOption.AllDirectories)
			.ToList();

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].FullName);
		result.Should().Contain(initialized[2].FullName);
	}

	[Theory]
	[AutoData]
	public void GetFiles_SearchOptionAllDirectories_ShouldReturnAllFiles(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(".", "*", SearchOption.AllDirectories)
			.ToList();

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[2].ToString());
	}

	[Theory]
#if NETFRAMEWORK
	[InlineAutoData(false, "")]
#else
	[InlineAutoData(true, "")]
#endif
	[InlineAutoData(true, "*")]
	[InlineAutoData(true, ".")]
	[InlineAutoData(true, "*.*")]
	[InlineData(true, "a*c", "abc")]
	[InlineData(true, "ab*c", "abc")]
	[InlineData(true, "abc?", "abc")]
	[InlineData(false, "ab?c", "abc")]
	[InlineData(false, "ac", "abc")]
	public void GetFiles_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
			.GetFiles(".", searchPattern).ToList();

		if (expectToBeFound)
		{
			result.Should().ContainSingle(
				fileName,
				$"it should match {searchPattern}");
		}
		else
		{
			result.Should()
				.BeEmpty($"{fileName} should not match {searchPattern}");
		}
	}

	[Fact]
	public void GetFiles_SearchPatternForFileWithoutExtension_ShouldWorkConsistently()
	{
		FileSystem.Initialize()
			.WithFile("file_without_extension")
			.WithFile("file.with.an.extension");

		string[] result = FileSystem.Directory.GetFiles(".", "*.");

		result.Length.Should().Be(1);
	}

	[Fact]
	public void
		GetFiles_SearchPatternWithDirectorySeparator_ShouldReturnFilesInSubdirectoryOnWindows()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(d => d
				.WithFile("bar.txt").Which(f => f.HasStringContent("inner")))
			.WithFile("bar.txt").Which(f => f.HasStringContent("outer"));

		string[] result1 = FileSystem.Directory.GetFiles(".", "foo\\*.txt");
		string[] result2 = FileSystem.Directory.GetFiles(".", ".\\*.txt");

		if (Test.RunsOnWindows)
		{
			result1.Length.Should().Be(1);
			FileSystem.File.ReadAllText(result1[0]).Should().Be("inner");
			result2.Length.Should().Be(1);
			FileSystem.File.ReadAllText(result2[0]).Should().Be("outer");
		}
		else
		{
			result1.Should().BeEmpty();
			result2.Should().BeEmpty();
		}
	}

	[Fact]
	public void GetFiles_SearchPatternWithTooManyAsterisk_ShouldWorkConsistently()
	{
		FileSystem.Initialize()
			.WithFile("result.test.001.txt");

		string[] result = FileSystem.Directory.GetFiles(".", "*.test.*.*.*.*");

		result.Length.Should().Be(1);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[AutoData]
	public void
		GetFiles_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(".",
				initialized[2].Name.ToUpper(CultureInfo.InvariantCulture),
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System,
				}).ToList();

		result.Count.Should().Be(1);
		result.Should().NotContain(initialized[0].ToString());
		result.Should().Contain(initialized[2].ToString());
	}
#endif

	[Theory]
	[AutoData]
	public void GetFiles_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.GetFiles(path, searchPattern)
				.FirstOrDefault();
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809,
			// The searchPattern is not included in .NET Framework
			messageContains: Test.IsNetFramework ? null : $"'{searchPattern}'");
	}

	[Theory]
	[AutoData]
	public void
		GetFiles_WithoutSearchString_ShouldReturnAllFilesInDirectSubdirectories(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(".")
			.ToList();

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[1].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[Theory]
	[AutoData]
	public void GetFiles_WithRelativePathAndSubfolders_ShouldReturnRelativeFilePath(
		string subfolder1, string subfolder2, string[] files)
	{
		string workingDirectory = FileSystem.Path.Combine(BasePath, subfolder1, subfolder2);
		FileSystem.Directory.CreateDirectory(workingDirectory);
		foreach (string file in files)
		{
			FileSystem.File.Create(FileSystem.Path.Combine(workingDirectory, file));
		}

		FileSystem.Directory.SetCurrentDirectory(subfolder1);
		IEnumerable<string> expectation =
			files.Select(f => FileSystem.Path.Combine("..", subfolder1, subfolder2, f));
		string path = FileSystem.Path.Combine("..", subfolder1, subfolder2);

		List<string> result = FileSystem.Directory.GetFiles(path).ToList();

		result.Should().BeEquivalentTo(expectation);
	}

	[Theory]
	[AutoData]
	public void GetFiles_WithSearchPattern_ShouldReturnMatchingFiles(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(".", initialized[0].Name)
			.ToList();

		result.Count.Should().Be(1);
		result.Should().Contain(initialized[0].ToString());
		result.Should().NotContain(initialized[1].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[Fact]
	public void
		GetFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFilesInSubdirectories()
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithAFile("foobar"))
				.WithASubdirectory().Initialized(s => s
					.WithAFile("foobar"))
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		IEnumerable<string> result = FileSystem.Directory
			.GetFiles(".", "*.foobar", SearchOption.AllDirectories)
			.ToArray();

		result.Count().Should().Be(2);
		result.Should().Contain(initialized[1].ToString());
		result.Should().Contain(initialized[3].ToString());
	}
}
