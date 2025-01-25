using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class GetDirectoriesTests
{
	[Theory]
	[AutoData]
	public void
		GetDirectories_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.GetDirectories(path).ToList());

		exception.Should().BeException<DirectoryNotFoundException>($"'{expectedPath}'",
			hResult: -2147024893);
		FileSystem.Should().NotHaveDirectory(path);
	}

	[Theory]
	[AutoData]
	public void
		GetDirectories_SearchOptionAllDirectories_FullPath_ShouldReturnAllSubdirectoriesWithFullPath(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.GetDirectories(baseDirectory.FullName, "*", SearchOption.AllDirectories)
			.ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(FileSystem.Path.Combine(baseDirectory.FullName, "foo"));
		result.Should()
			.Contain(FileSystem.Path.Combine(baseDirectory.FullName, "foo", "xyz"));
		result.Should().Contain(FileSystem.Path.Combine(baseDirectory.FullName, "bar"));
	}

	[Theory]
	[AutoData]
	public void
		GetDirectories_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(path, "foo", "xyz"));
		result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
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
	public void GetDirectories_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string subdirectoryName)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory("foo");
		baseDirectory.CreateSubdirectory(subdirectoryName);

		List<string> result = FileSystem.Directory
			.GetDirectories("foo", searchPattern).ToList();

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
	[Theory]
	[AutoData]
	public void
		GetDirectories_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.GetDirectories(path, "XYZ",
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System,
				}).ToList();

		result.Count.Should().Be(1);
		result.Should().NotContain(FileSystem.Path.Combine(path, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(path, "foo", "xyz"));
		result.Should().NotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

	[Theory]
	[AutoData]
	public void GetDirectories_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.GetDirectories(path, searchPattern)
				.FirstOrDefault();
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[Theory]
	[AutoData]
	public void
		GetDirectories_WithoutSearchString_ShouldReturnAllDirectSubdirectories(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory.GetDirectories(path).ToList();

		result.Count.Should().Be(2);
		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().NotContain(FileSystem.Path.Combine(path, "foo", "xyz"));
		result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
	}

	[Fact]
	public void GetDirectories_WithRelativePath_ShouldReturnRelativePaths()
	{
		string path = $"foo{FileSystem.Path.DirectorySeparatorChar}bar";
		FileSystem.Directory.CreateDirectory(path);

		string[] result = FileSystem.Directory.GetDirectories("foo");

		result.Should().BeEquivalentTo(path);
	}

	[Theory]
	[AutoData]
	public void GetDirectories_WithSearchPattern_ShouldReturnMatchingSubdirectory(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");

		IEnumerable<string> result =
			FileSystem.Directory.GetDirectories(path, "foo");

		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
	}

	[Theory]
	[AutoData]
	public void
		GetDirectories_WithSearchPatternInSubdirectory_ShouldReturnMatchingSubdirectory(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar/xyz");

		IEnumerable<string> result = FileSystem.Directory
			.GetDirectories(path, "xyz", SearchOption.AllDirectories);

		result.Count().Should().Be(2);
	}
}
