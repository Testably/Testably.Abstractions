using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class EnumerateDirectoriesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void EnumerateDirectories_AbsolutePath_ShouldNotIncludeTrailingSlash()
	{
		FileSystem.Directory.CreateDirectory("foo");
		FileSystem.Directory.CreateDirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(BasePath)
			.ToList();

		result.Should().Contain(FileSystem.Path.Combine(BasePath, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(BasePath, "bar"));
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateDirectories_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = System.IO.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.EnumerateDirectories(path).ToList());

		exception.Should().BeException<DirectoryNotFoundException>(
			$"'{expectedPath}'",
			hResult: -2147024893);
		FileSystem.Should().NotHaveDirectory(path);
	}

	[SkippableFact]
	public void EnumerateDirectories_RelativePath_ShouldNotIncludeTrailingSlash()
	{
		string path = ".";
		FileSystem.Directory.CreateDirectory("foo");
		FileSystem.Directory.CreateDirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path)
			.ToList();

		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
	}

	[SkippableFact]
	public void
		EnumerateDirectories_RelativePathToParentDirectory_ShouldNotIncludeTrailingSlash()
	{
		string path = "foo/..";
		FileSystem.Directory.CreateDirectory("foo");
		FileSystem.Directory.CreateDirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path)
			.ToList();

		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateDirectories_SearchOptionAllDirectories_FullPath_ShouldReturnAllSubdirectoriesWithFullPath(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(baseDirectory.FullName, "*", SearchOption.AllDirectories)
			.ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(FileSystem.Path.Combine(baseDirectory.FullName, "foo"));
		result.Should()
			.Contain(FileSystem.Path.Combine(baseDirectory.FullName, "foo", "xyz"));
		result.Should().Contain(FileSystem.Path.Combine(baseDirectory.FullName, "bar"));
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateDirectories_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "*", SearchOption.AllDirectories).ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(path, "foo", "xyz"));
		result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
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
	public void EnumerateDirectories_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string subdirectoryName)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory("foo");
		baseDirectory.CreateSubdirectory(subdirectoryName);

		List<string> result = FileSystem.Directory
			.EnumerateDirectories("foo", searchPattern).ToList();

		if (expectToBeFound)
		{
			result.Should().ContainSingle(
				FileSystem.Path.Combine("foo", subdirectoryName),
				$"it should match {searchPattern}");
		}
		else
		{
			result.Should()
				.BeEmpty($"{subdirectoryName} should not match {searchPattern}");
		}
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableTheory]
	[AutoData]
	public void
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "XYZ",
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System
				}).ToList();

		result.Count.Should().Be(1);
		result.Should().NotContain(FileSystem.Path.Combine(path, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(path, "foo", "xyz"));
		result.Should().NotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void EnumerateDirectories_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.EnumerateDirectories(path, searchPattern)
				.FirstOrDefault();
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateDirectories_WithoutSearchString_ShouldReturnAllDirectSubdirectories(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory.EnumerateDirectories(path).ToList();

		result.Count.Should().Be(2);
		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().NotContain(FileSystem.Path.Combine(path, "foo", "xyz"));
		result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
	}

	[SkippableTheory]
	[AutoData]
	public void EnumerateDirectories_WithSearchPattern_ShouldReturnMatchingSubdirectory(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");

		IEnumerable<string> result =
			FileSystem.Directory.EnumerateDirectories(path, "foo");

		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateDirectories_WithSearchPatternInSubdirectory_ShouldReturnMatchingSubdirectory(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar/xyz");

		IEnumerable<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "xyz", SearchOption.AllDirectories);

		result.Count().Should().Be(2);
	}

	[SkippableFact]
	public void EnumerateDirectories_WithTrailingSlash_ShouldEnumerateSubdirectories()
	{
		string queryPath = @"Folder\";
		string expectedPath = @"Folder\SubFolder";
		FileSystem.Directory.CreateDirectory("Folder/SubFolder");

		IEnumerable<string> actualResult = FileSystem.Directory.EnumerateDirectories(queryPath);

		actualResult.Should().BeEquivalentTo(expectedPath);
	}
}
