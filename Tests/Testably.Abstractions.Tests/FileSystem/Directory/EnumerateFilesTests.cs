using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class EnumerateFilesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
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
		   .WithSubdirectory(path.ToUpperInvariant()).Initialized(s => s
			   .WithAFile());

		string[] result = FileSystem.Directory.GetFiles(path.ToLowerInvariant());

		result.Length.Should().Be(1);
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFiles_SearchOptionAllDirectories_FullPath_ShouldReturnAllFilesWithFullPath(
			string path)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(FileSystem.Directory.GetCurrentDirectory(),
				"*", SearchOption.AllDirectories)
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
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(".", "*", SearchOption.AllDirectories)
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
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(".",
				initialized[2].Name.ToUpper(),
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
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(".")
		   .ToList();

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
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFiles(".", initialized[0].Name)
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
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
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