using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class GetDirectoriesTests
{
	[Theory]
	[AutoData]
	public async Task
		GetDirectories_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);

		void Act()
			=> FileSystem.Directory.GetDirectories(path);

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{expectedPath}'").And
			.WithHResult(-2147024893);
		await That(FileSystem.Directory.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task
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

		await That(result.Count).IsEqualTo(3);
		await That(result).Contains(FileSystem.Path.Combine(baseDirectory.FullName, "foo"));
		await That(result).Contains(FileSystem.Path.Combine(baseDirectory.FullName, "foo", "xyz"));
		await That(result).Contains(FileSystem.Path.Combine(baseDirectory.FullName, "bar"));
	}

	[Theory]
	[AutoData]
	public async Task GetDirectories_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();

		await That(result.Count).IsEqualTo(3);
		await That(result).Contains(FileSystem.Path.Combine(path, "foo"));
		await That(result).Contains(FileSystem.Path.Combine(path, "foo", "xyz"));
		await That(result).Contains(FileSystem.Path.Combine(path, "bar"));
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
	public async Task GetDirectories_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string subdirectoryName)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory("foo");
		baseDirectory.CreateSubdirectory(subdirectoryName);

		List<string> result = FileSystem.Directory
			.GetDirectories("foo", searchPattern).ToList();

		if (expectToBeFound)
		{
			await That(result).HasSingle().Which
				.IsEqualTo(FileSystem.Path.Combine("foo", subdirectoryName))
				.Because($"it should match {searchPattern}");
		}
		else
		{
			await That(result).IsEmpty()
				.Because($"{subdirectoryName} should not match {searchPattern}");
		}
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[AutoData]
	public async Task GetDirectories_WithEnumerationOptions_ShouldConsiderSetOptions(
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

		await That(result.Count).IsEqualTo(1);
		await That(result).DoesNotContain(FileSystem.Path.Combine(path, "foo"));
		await That(result).Contains(FileSystem.Path.Combine(path, "foo", "xyz"));
		await That(result).DoesNotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

	[Theory]
	[AutoData]
	public async Task GetDirectories_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		void Act()
		{
			_ = FileSystem.Directory.GetDirectories(path, searchPattern)
				.FirstOrDefault();
		}

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Theory]
	[AutoData]
	public async Task GetDirectories_WithoutSearchString_ShouldReturnAllDirectSubdirectories(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory.GetDirectories(path).ToList();

		await That(result.Count).IsEqualTo(2);
		await That(result).Contains(FileSystem.Path.Combine(path, "foo"));
		await That(result).DoesNotContain(FileSystem.Path.Combine(path, "foo", "xyz"));
		await That(result).Contains(FileSystem.Path.Combine(path, "bar"));
	}

	[Fact]
	public async Task GetDirectories_WithRelativePath_ShouldReturnRelativePaths()
	{
		string path = $"foo{FileSystem.Path.DirectorySeparatorChar}bar";
		FileSystem.Directory.CreateDirectory(path);

		string[] result = FileSystem.Directory.GetDirectories("foo");

		await That(result).IsEqualTo([path]);
	}

	[Theory]
	[AutoData]
	public async Task GetDirectories_WithSearchPattern_ShouldReturnMatchingSubdirectory(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");

		IEnumerable<string> result =
			FileSystem.Directory.GetDirectories(path, "foo");

		await That(result).Contains(FileSystem.Path.Combine(path, "foo"));
	}

	[Theory]
	[AutoData]
	public async Task
		GetDirectories_WithSearchPatternInSubdirectory_ShouldReturnMatchingSubdirectory(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar/xyz");

		IEnumerable<string> result = FileSystem.Directory
			.GetDirectories(path, "xyz", SearchOption.AllDirectories);

		await That(result).HasCount(2);
	}
}
