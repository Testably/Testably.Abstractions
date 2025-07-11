using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
using System.Globalization;
#endif

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class EnumerateFileSystemInfosTests
{
	[Theory]
	[AutoData]
	public async Task
		EnumerateFileSystemEntries_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);

		void Act() =>
			_ = FileSystem.Directory.EnumerateFileSystemEntries(path).ToList();

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{expectedPath}'").And
			.WithHResult(-2147024893);
		await That(FileSystem.Directory.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task
		EnumerateFileSystemEntries_SearchOptionAllDirectories_FullPath_ShouldReturnAllFileSystemEntriesWithFullPath(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.EnumerateFileSystemEntries(
				FileSystem.Directory.GetCurrentDirectory(),
				"*",
				SearchOption.AllDirectories)
			.ToList();

		await That(result.Count).IsEqualTo(3);
		await That(result).Contains(initialized[0].FullName);
		await That(result).Contains(initialized[1].FullName);
		await That(result).Contains(initialized[2].FullName);
	}

	[Theory]
	[AutoData]
	public async Task
		EnumerateFileSystemEntries_SearchOptionAllDirectories_ShouldReturnAllFileSystemEntries(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.EnumerateFileSystemEntries(".", "*", SearchOption.AllDirectories)
			.ToList();

		await That(result.Count).IsEqualTo(3);
		await That(result).Contains(initialized[0].ToString());
		await That(result).Contains(initialized[1].ToString());
		await That(result).Contains(initialized[2].ToString());
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
	public async Task EnumerateFileSystemEntries_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
			.EnumerateFileSystemEntries(".", searchPattern).ToList();

		if (expectToBeFound)
		{
			await That(result).HasSingle().Which.EndsWith(fileName)
				.Because($"it should match {searchPattern}");
		}
		else
		{
			await That(result).IsEmpty().Because($"{fileName} should not match {searchPattern}");
		}
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[AutoData]
	public async Task EnumerateFileSystemEntries_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.EnumerateFileSystemEntries(".",
				initialized[2].Name.ToUpper(CultureInfo.InvariantCulture),
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System,
				}).ToList();

		await That(result.Count).IsEqualTo(1).Because($"{initialized[2]} should be found.");
		await That(result).DoesNotContain(initialized[0].ToString());
		await That(result).Contains(initialized[2].ToString());
	}
#endif

	[Theory]
	[AutoData]
	public async Task
		EnumerateFileSystemEntries_WithFileNameStartingWithDirectoryName_ShouldNotBeIncluded(
			string path)
	{
		FileSystem.Initialize()
			.WithSubdirectory(path)
			.WithFile($"{path}.txt");

		List<string> result = FileSystem.Directory
			.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories)
			.ToList();

		await That(result).IsEmpty();
	}

	[Theory]
	[AutoData]
	public async Task EnumerateFileSystemEntries_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		void Act()
		{
			_ = FileSystem.Directory.EnumerateFileSystemEntries(path, searchPattern)
				.FirstOrDefault();
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithMessageContaining(
				// The searchPattern is not included in .NET Framework
				Test.IsNetFramework ? null : $"'{searchPattern}'");
	}

	[Theory]
	[AutoData]
	public async Task
		EnumerateFileSystemEntries_WithoutSearchString_ShouldReturnAllFileSystemEntriesInDirectSubdirectories(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result =
			FileSystem.Directory
				.EnumerateFileSystemEntries(".")
				.ToList();

		await That(result.Count).IsEqualTo(3);
		await That(result).Contains(initialized[0].ToString());
		await That(result).Contains(initialized[1].ToString());
		await That(result).Contains(initialized[2].ToString());
		await That(result).DoesNotContain(initialized[3].ToString());
	}

	[Theory]
	[AutoData]
	public async Task
		EnumerateFileSystemEntries_WithSearchPattern_ShouldReturnMatchingFileSystemEntries(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.EnumerateFileSystemEntries(".", initialized[0].Name)
			.ToList();

		await That(result.Count).IsEqualTo(1);
		await That(result).Contains(initialized[0].ToString());
		await That(result).DoesNotContain(initialized[1].ToString());
		await That(result).DoesNotContain(initialized[3].ToString());
	}

	[Fact]
	public async Task
		EnumerateFileSystemEntries_WithSearchPatternInSubdirectory_ShouldReturnMatchingFileSystemEntriesInSubdirectories()
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
			.EnumerateFileSystemEntries(".", "*.foobar", SearchOption.AllDirectories)
			.ToArray();

		await That(result).HasCount(2);
		await That(result).Contains(initialized[1].ToString());
		await That(result).Contains(initialized[3].ToString());
	}
}
