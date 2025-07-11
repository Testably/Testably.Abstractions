using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class EnumerateDirectoriesTests
{
	[Theory]
	[AutoData]
	public async Task EnumerateDirectories_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(path).Initialized(s => s
					.WithSubdirectory("foo/xyz")
					.WithSubdirectory("bar"));
		IDirectoryInfo baseDirectory =
			(IDirectoryInfo)initialized[0];

		IDirectoryInfo[] result = baseDirectory
			.EnumerateDirectories("*", SearchOption.AllDirectories).ToArray();

		await That(result.Length).IsEqualTo(3);
		await That(result).Contains(d => string.Equals(d.Name, "foo", StringComparison.Ordinal));
		await That(result).Contains(d => string.Equals(d.Name, "bar", StringComparison.Ordinal));
		await That(result).Contains(d => string.Equals(d.Name, "xyz", StringComparison.Ordinal));
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
	public async Task EnumerateDirectories_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string subdirectoryName)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory("foo");
		baseDirectory.CreateSubdirectory(subdirectoryName);

		IDirectoryInfo[] result = baseDirectory
			.EnumerateDirectories(searchPattern).ToArray();

		if (expectToBeFound)
		{
			await That(result).HasSingle().Matching(d
					=> string.Equals(d.Name, subdirectoryName, StringComparison.Ordinal))
				.Because($"it should match '{searchPattern}'");
		}
		else
		{
			await That(result).IsEmpty()
				.Because($"{subdirectoryName} should not match '{searchPattern}'");
		}
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[AutoData]
	public async Task EnumerateDirectories_WithEnumerationOptions_ShouldConsiderSetOptions(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		IDirectoryInfo[] result = baseDirectory
			.EnumerateDirectories("XYZ",
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System,
				}).ToArray();

		await That(result.Length).IsEqualTo(1);
		await That(result).DoesNotContain(d
			=> string.Equals(d.Name, "foo", StringComparison.Ordinal));
		await That(result).Contains(d => string.Equals(d.Name, "xyz", StringComparison.Ordinal));
		await That(result).DoesNotContain(d
			=> string.Equals(d.Name, "bar", StringComparison.Ordinal));
	}
#endif

	[Theory]
	[AutoData]
	public async Task EnumerateDirectories_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.DirectoryInfo.New(path);
		string searchPattern = "foo\0bar";

		void Act()
		{
			_ = baseDirectory.EnumerateDirectories(searchPattern).FirstOrDefault();
		}

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Theory]
	[AutoData]
	public async Task EnumerateDirectories_WithoutSearchString_ShouldReturnAllDirectSubdirectories(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar");

		IDirectoryInfo[] result = baseDirectory
			.EnumerateDirectories().ToArray();

		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(d => string.Equals(d.Name, "foo", StringComparison.Ordinal));
		await That(result)
			.DoesNotContain(d => string.Equals(d.Name, "xyz", StringComparison.Ordinal));
		await That(result).Contains(d => string.Equals(d.Name, "bar", StringComparison.Ordinal));
	}

	[Theory]
	[AutoData]
	public async Task EnumerateDirectories_WithSearchPattern_ShouldReturnMatchingSubdirectory(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");

		IEnumerable<IDirectoryInfo> result = baseDirectory
			.EnumerateDirectories("foo");

		await That(result).HasSingle()
			.Matching(d => string.Equals(d.Name, "foo", StringComparison.Ordinal));
	}

	[Theory]
	[AutoData]
	public async Task
		EnumerateDirectories_WithSearchPatternInSubdirectory_ShouldReturnMatchingSubdirectory(
			string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo/xyz");
		baseDirectory.CreateSubdirectory("bar/xyz");

		IEnumerable<IDirectoryInfo> result = baseDirectory
			.EnumerateDirectories("xyz", SearchOption.AllDirectories);

		await That(result).HasCount(2);
	}
}
