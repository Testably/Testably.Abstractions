using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public class GetFileSystemInfosTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task GetFileSystemInfos_SearchOptionAllFiles_ShouldReturnAllFiles(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(path).Initialized(s => s
					.WithASubdirectory().Initialized(d => d
						.WithAFile()
						.WithAFile())
					.WithASubdirectory()
					.WithAFile());
		IDirectoryInfo baseDirectory =
			(IDirectoryInfo)initialized[0];

		IFileSystemInfo[] result = baseDirectory
			.GetFileSystemInfos("*", SearchOption.AllDirectories);

		await That(result.Length).IsEqualTo(5);
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[1].Name, StringComparison.Ordinal));
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[2].Name, StringComparison.Ordinal));
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[3].Name, StringComparison.Ordinal));
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[4].Name, StringComparison.Ordinal));
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[5].Name, StringComparison.Ordinal));
	}

	[Test]
#if NETFRAMEWORK
	[AutoArguments(false, "")]
#else
	[AutoArguments(true, "")]
#endif
	[AutoArguments(true, "*")]
	[AutoArguments(true, ".")]
	[AutoArguments(true, "*.*")]
	[Arguments(true, "a*c", "abc")]
	[Arguments(true, "ab*c", "abc")]
	[Arguments(true, "abc?", "abc")]
	[Arguments(false, "ab?c", "abc")]
	[Arguments(false, "ac", "abc")]
	public async Task GetFileSystemInfos_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile(fileName)
				.BaseDirectory;

		IFileSystemInfo[] result = baseDirectory
			.GetFileSystemInfos(searchPattern);

		if (expectToBeFound)
		{
			await That(result).HasSingle()
				.Matching(d => string.Equals(d.Name, fileName, StringComparison.Ordinal))
				.Because($"it should match '{searchPattern}'");
		}
		else
		{
			await That(result).IsEmpty().Because($"{fileName} should not match '{searchPattern}'");
		}
	}

	[Test]
	[AutoArguments]
	public async Task GetFileSystemInfos_ShouldMatchTypes(string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(path).Initialized(s => s
					.WithASubdirectory()
					.WithAFile());
		IDirectoryInfo baseDirectory =
			(IDirectoryInfo)initialized[0];

		IFileSystemInfo[] result = baseDirectory
			.GetFileSystemInfos("*");

		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[1].Name, StringComparison.Ordinal) &&
			   d is IDirectoryInfo);
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[2].Name, StringComparison.Ordinal) &&
			   d is IFileInfo);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	public async Task GetFileSystemInfos_WithEnumerationOptions_ShouldConsiderSetOptions()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithFile("xyz"))
				.WithAFile()
				.BaseDirectory;

		IFileSystemInfo[] result = baseDirectory
			.GetFileSystemInfos("XYZ",
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System,
				});

		await That(result.Length).IsEqualTo(1);
		await That(result).DoesNotContain(d
			=> string.Equals(d.Name, "foo", StringComparison.Ordinal));
		await That(result).Contains(d => string.Equals(d.Name, "xyz", StringComparison.Ordinal));
		await That(result).DoesNotContain(d
			=> string.Equals(d.Name, "bar", StringComparison.Ordinal));
	}
#endif

	[Test]
	[AutoArguments]
	public async Task GetFileSystemInfos_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.DirectoryInfo.New(path);
		string searchPattern = "foo\0bar";

		void Act()
		{
			_ = baseDirectory.GetFileSystemInfos(searchPattern).FirstOrDefault();
		}

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Test]
	public async Task
		GetFileSystemInfos_WithoutSearchString_ShouldReturnAllDirectFilesAndDirectories()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithSubdirectory("muh").Initialized(s => s
					.WithFile("xyz"))
				.WithFile("bar")
				.BaseDirectory;

		IFileSystemInfo[] result = baseDirectory
			.GetFileSystemInfos();

		await That(result.Length).IsEqualTo(3);
		await That(result).Contains(d => string.Equals(d.Name, "foo", StringComparison.Ordinal));
		await That(result).Contains(d => string.Equals(d.Name, "muh", StringComparison.Ordinal));
		await That(result)
			.DoesNotContain(d => string.Equals(d.Name, "xyz", StringComparison.Ordinal));
		await That(result).Contains(d => string.Equals(d.Name, "bar", StringComparison.Ordinal));
	}

	[Test]
	public async Task GetFileSystemInfos_WithSearchPattern_ShouldReturnMatchingFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithFile("bar")
				.BaseDirectory;

		IEnumerable<IFileSystemInfo> result = baseDirectory
			.GetFileSystemInfos("foo");

		await That(result).HasSingle()
			.Matching(d => string.Equals(d.Name, "foo", StringComparison.Ordinal));
		await That(result.Count()).IsEqualTo(1);
	}

	[Test]
	public async Task
		GetFileSystemInfos_WithSearchPatternInSubdirectory_ShouldReturnMatchingFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithFile("xyz"))
				.WithASubdirectory().Initialized(s => s
					.WithFile("xyz"))
				.WithSubdirectory("xyz").Initialized(s => s
					.WithAFile())
				.BaseDirectory;

		IEnumerable<IFileSystemInfo> result = baseDirectory
			.GetFileSystemInfos("xyz", SearchOption.AllDirectories);

		await That(result.Count()).IsEqualTo(3);
	}
}
