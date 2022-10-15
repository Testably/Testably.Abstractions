using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void
		EnumerateDirectories_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = System.IO.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.EnumerateDirectories(path).ToList());

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.Message.Should()
		   .Be($"Could not find a part of the path '{expectedPath}'.");
		FileSystem.Directory.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void
		EnumerateDirectories_SearchOptionAllDirectories_FullPath_ShouldReturnAllSubdirectoriesWithFullPath(
			string path)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
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
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void
		EnumerateDirectories_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
			string path)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
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
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void EnumerateDirectories_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string subdirectoryName)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
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
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
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
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void EnumerateDirectories_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.EnumerateDirectories(path, searchPattern)
			   .FirstOrDefault();
		});

		exception.Should().BeOfType<ArgumentException>();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void
		EnumerateDirectories_WithoutSearchString_ShouldReturnAllDirectSubdirectories(
			string path)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
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
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void EnumerateDirectories_WithSearchPattern_ShouldReturnMatchingSubdirectory(
		string path)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");

		IEnumerable<string> result =
			FileSystem.Directory.EnumerateDirectories(path, "foo");

		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.EnumerateDirectories))]
	public void
		EnumerateDirectories_WithSearchPatternInSubdirectory_ShouldReturnMatchingSubdirectory(
			string path)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar/xyz");

		IEnumerable<string> result = FileSystem.Directory
		   .EnumerateDirectories(path, "xyz", SearchOption.AllDirectories);

		result.Count().Should().Be(2);
	}
}