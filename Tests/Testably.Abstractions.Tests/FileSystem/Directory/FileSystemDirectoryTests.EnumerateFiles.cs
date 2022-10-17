using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFiles_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = System.IO.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.EnumerateFiles(path).ToList());

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.Message.Should().Contain($"'{expectedPath}'.");
		FileSystem.Directory.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void EnumerateFiles_Path_ShouldBeCaseInsensitiveOnWindows(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Initialize()
		   .WithSubdirectory(path.ToUpper()).Initialized(s => s
			   .WithAFile());

		string[] result = FileSystem.Directory.GetFiles(path.ToLower());

		result.Length.Should().Be(1);
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFiles_SearchOptionAllDirectories_FullPath_ShouldReturnAllFilesWithFullPath(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(FileSystem.Path.GetFullPath(path), "*",
				SearchOption.AllDirectories)
		   .ToList();

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].FullName);
		result.Should().Contain(initialized[2].FullName);
	}

	[SkippableTheory]
	[AutoData]
	public void EnumerateFiles_SearchOptionAllDirectories_ShouldReturnAllFiles(
		string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(path, "*", SearchOption.AllDirectories)
		   .ToList();

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[2].ToString());
	}

	[SkippableTheory]
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
	public void EnumerateFiles_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(".", searchPattern).ToList();

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

	[SkippableFact]
	public void
		EnumerateFiles_SearchPatternForFileWithoutExtension_ShouldWorkConsistently()
	{
		FileSystem.Initialize()
		   .WithFile("file_without_extension")
		   .WithFile("file.with.an.extension");

		string[] result = FileSystem.Directory.GetFiles(".", "*.");

		result.Length.Should().Be(1);
	}

	[SkippableFact]
	public void EnumerateFiles_SearchPatternWithTooManyAsterisk_ShouldWorkConsistently()
	{
		FileSystem.Initialize()
		   .WithFile("result.test.001.txt");

		string[] result = FileSystem.Directory.GetFiles(".", "*.test.*.*.*.*");

		result.Length.Should().Be(1);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFiles_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(path, initialized[2].Name.ToUpper(),
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System
				}).ToList();

		result.Count.Should().Be(1);
		result.Should().NotContain(initialized[0].ToString());
		result.Should().Contain(initialized[2].ToString());
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void EnumerateFiles_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.EnumerateFiles(path, searchPattern)
			   .FirstOrDefault();
		});

#if NETFRAMEWORK
		// The searchPattern is not included in .NET Framework
		exception.Should().BeOfType<ArgumentException>();
#else
		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain($"'{searchPattern}'");
#endif
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFiles_WithoutSearchString_ShouldReturnAllFilesInDirectSubdirectories(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory.EnumerateFiles(path).ToList();

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[1].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[SkippableTheory]
	[AutoData]
	public void EnumerateFiles_WithSearchPattern_ShouldReturnMatchingFiles(
		string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(path, initialized[0].Name)
		   .ToList();

		result.Count.Should().Be(1);
		result.Should().Contain(initialized[0].ToString());
		result.Should().NotContain(initialized[1].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[SkippableFact]
	public void
		EnumerateFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFilesInSubdirectories()
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile("foobar"))
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile("foobar"))
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		IEnumerable<string> result = FileSystem.Directory
		   .EnumerateFiles(".", "*.foobar", SearchOption.AllDirectories)
		   .ToArray();

		result.Count().Should().Be(2);
		result.Should().Contain(initialized[1].ToString());
		result.Should().Contain(initialized[3].ToString());
	}
}