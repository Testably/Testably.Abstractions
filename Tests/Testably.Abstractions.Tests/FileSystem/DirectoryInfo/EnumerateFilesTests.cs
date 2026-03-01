using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public class EnumerateFilesTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task EnumerateFiles_SearchOptionAllFiles_ShouldReturnAllFiles(
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

		IFileInfo[] result = baseDirectory
			.EnumerateFiles("*", SearchOption.AllDirectories).ToArray();

		await That(result.Length).IsEqualTo(3);
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[2].Name, StringComparison.Ordinal));
		await That(result).Contains(d
			=> string.Equals(d.Name, initialized[3].Name, StringComparison.Ordinal));
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
	public async Task EnumerateFiles_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile(fileName)
				.BaseDirectory;

		IFileInfo[] result = baseDirectory
			.EnumerateFiles(searchPattern).ToArray();

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

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	public async Task EnumerateFiles_WithEnumerationOptions_ShouldConsiderSetOptions()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithFile("xyz"))
				.WithAFile()
				.BaseDirectory;

		IFileInfo[] result = baseDirectory
			.EnumerateFiles("XYZ",
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

	[Test]
	[AutoArguments]
	public async Task EnumerateFiles_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.DirectoryInfo.New(path);
		string searchPattern = "foo\0bar";

		void Act()
		{
			_ = baseDirectory.EnumerateFiles(searchPattern).FirstOrDefault();
		}

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Test]
	public async Task EnumerateFiles_WithoutSearchString_ShouldReturnAllDirectFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithASubdirectory().Initialized(s => s
					.WithFile("xyz"))
				.WithFile("bar")
				.BaseDirectory;

		IFileInfo[] result = baseDirectory
			.EnumerateFiles().ToArray();

		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(d => string.Equals(d.Name, "foo", StringComparison.Ordinal));
		await That(result)
			.DoesNotContain(d => string.Equals(d.Name, "xyz", StringComparison.Ordinal));
		await That(result).Contains(d => string.Equals(d.Name, "bar", StringComparison.Ordinal));
	}

	[Test]
	public async Task EnumerateFiles_WithSearchPattern_ShouldReturnMatchingFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithFile("bar")
				.BaseDirectory;

		IEnumerable<IFileInfo> result = baseDirectory
			.EnumerateFiles("foo").ToArray();

		await That(result).HasSingle()
			.Matching(d => string.Equals(d.Name, "foo", StringComparison.Ordinal));
		await That(result.Count()).IsEqualTo(1);
	}

	[Test]
	public async Task
		EnumerateFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFiles()
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

		IEnumerable<IFileInfo> result = baseDirectory
			.EnumerateFiles("xyz", SearchOption.AllDirectories);

		await That(result).HasCount(2);
	}

	[Test]
	public async Task
		EnumerateFiles_WithSearchPatternWithDirectorySeparator_ShouldReturnFilesInSubdirectoryOnWindows()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithSubdirectory("foo").Initialized(d => d
					.WithFile("bar.txt").Which(f => f.HasStringContent("inner")))
				.WithFile("bar.txt").Which(f => f.HasStringContent("outer"))
				.BaseDirectory;

		List<IFileInfo> result1 = baseDirectory.EnumerateFiles("foo\\*.txt").ToList();
		List<IFileInfo> result2 = baseDirectory.EnumerateFiles(".\\*.txt").ToList();

		if (Test.RunsOnWindows)
		{
			await That(result1.Count).IsEqualTo(1);
			await That(FileSystem.File.ReadAllText(result1.Single().FullName)).IsEqualTo("inner");
			await That(result2.Count).IsEqualTo(1);
			await That(FileSystem.File.ReadAllText(result2.Single().FullName)).IsEqualTo("outer");
		}
		else
		{
			await That(result1).IsEmpty();
			await That(result2).IsEmpty();
		}
	}
}
