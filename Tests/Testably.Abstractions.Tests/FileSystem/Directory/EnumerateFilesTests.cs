using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public class EnumerateFilesTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task
		EnumerateFiles_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);

		void Act() =>
			_ = FileSystem.Directory.EnumerateFiles(path).ToList();

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{expectedPath}'").And
			.WithHResult(-2147024893);
		await That(FileSystem.Directory.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task
		EnumerateFiles_MissingDirectory_ShouldThrowDirectoryNotFoundExceptionImmediately(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);

		void Act() =>
			_ = FileSystem.Directory.EnumerateFiles(path);

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{expectedPath}'").And
			.WithHResult(-2147024893);
		await That(FileSystem.Directory.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task
		EnumerateFiles_SearchOptionAllDirectories_FullPath_ShouldReturnAllFilesWithFullPath(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.EnumerateFiles(FileSystem.Directory.GetCurrentDirectory(),
				"*", SearchOption.AllDirectories)
			.ToList();

		await That(result.Count).IsEqualTo(2);
		await That(result).Contains(initialized[0].FullName);
		await That(result).Contains(initialized[2].FullName);
	}

	[Test]
	[AutoArguments]
	public async Task EnumerateFiles_SearchOptionAllDirectories_ShouldReturnAllFiles(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.EnumerateFiles(".", "*", SearchOption.AllDirectories)
			.ToList();

		await That(result.Count).IsEqualTo(2);
		await That(result).Contains(initialized[0].ToString());
		await That(result).Contains(initialized[2].ToString());
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
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
			.EnumerateFiles(".", searchPattern).ToList();

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

	[Test]
	[AutoArguments(true, "*.xls", ".xls")]
	[AutoArguments(false, "*.x", ".xls")]
#if NETFRAMEWORK
	[AutoArguments(true, "*.xls", ".xlsx")]
#else
	[AutoArguments(false, "*.xls", ".xlsx")]
#endif
	[AutoArguments(false, "foo.x", ".xls", "foo")]
	[AutoArguments(false, "?.xls", ".xlsx", "a")]
	public async Task EnumerateFiles_SearchPattern_WithFileExtension_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string extension,
		string fileNameWithoutExtension)
	{
		string fileName = $"{fileNameWithoutExtension}{extension}";
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
			.EnumerateFiles(".", searchPattern).ToList();

		if (expectToBeFound)
		{
			await That(result).HasSingle().Which.EndsWith(extension)
				.Because($"it should match {searchPattern}");
		}
		else
		{
			await That(result).IsEmpty().Because($"{extension} should not match {searchPattern}");
		}
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	[AutoArguments]
	public async Task EnumerateFiles_WithEnumerationOptions_ShouldConsiderAttributesToSkip(
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			AttributesToSkip = FileAttributes.ReadOnly,
		};
		FileSystem.Directory.CreateDirectory(path);
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "foo"), "");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "bar"), "");
		FileSystem.File.SetAttributes(FileSystem.Path.Combine(path, "bar"),
			FileAttributes.ReadOnly);

		List<string> result = FileSystem.Directory
			.EnumerateFiles(path, "*", enumerationOptions).ToList();

		await That(result.Count).IsEqualTo(1);
		await That(result).Contains(FileSystem.Path.Combine(path, "foo"));
		await That(result).DoesNotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	[AutoArguments(MatchCasing.CaseInsensitive)]
	[AutoArguments(MatchCasing.CaseSensitive)]
	public async Task EnumerateFiles_WithEnumerationOptions_ShouldConsiderMatchCasing(
			MatchCasing matchCasing,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			MatchCasing = matchCasing,
		};
		FileSystem.Directory.CreateDirectory(path);
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "foo"), "");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "bar"), "");

		List<string> result = FileSystem.Directory
			.EnumerateFiles(path, "FOO", enumerationOptions).ToList();

		await That(result.Count).IsEqualTo(matchCasing == MatchCasing.CaseInsensitive ? 1 : 0);
		if (matchCasing == MatchCasing.CaseInsensitive)
		{
			await That(result).Contains(FileSystem.Path.Combine(path, "foo"));
		}

		await That(result).DoesNotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	[AutoArguments(MatchType.Simple)]
	[AutoArguments(MatchType.Win32)]
	public async Task EnumerateFiles_WithEnumerationOptions_ShouldConsiderMatchType(
			MatchType matchType,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			MatchType = matchType,
		};
		FileSystem.Directory.CreateDirectory(path);
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "foo"), "");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "bar"), "");

		List<string> result = FileSystem.Directory
			.EnumerateFiles(path, "*.", enumerationOptions).ToList();

		await That(result.Count).IsEqualTo(matchType == MatchType.Win32 ? 2 : 0);
		if (matchType == MatchType.Win32)
		{
			await That(result).Contains(FileSystem.Path.Combine(path, "foo"));
			await That(result).Contains(FileSystem.Path.Combine(path, "bar"));
		}
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	[AutoArguments(true, 0)]
	[AutoArguments(true, 1)]
	[AutoArguments(true, 2)]
	[AutoArguments(true, 3)]
	[AutoArguments(false, 2)]
	public async Task EnumerateFiles_WithEnumerationOptions_ShouldConsiderMaxRecursionDepthWhenRecurseSubdirectoriesIsSet(
			bool recurseSubdirectories,
			int maxRecursionDepth,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			MaxRecursionDepth = maxRecursionDepth,
			RecurseSubdirectories = recurseSubdirectories,
		};
		FileSystem.Directory.CreateDirectory(
			FileSystem.Path.Combine(path, "a", "b", "c", "d", "e"));
		FileSystem.File.WriteAllText(
			FileSystem.Path.Combine(path, "a", "b", "c", "d", "e", "foo"), "");
		FileSystem.File.WriteAllText(
			FileSystem.Path.Combine(path, "a", "b", "c", "d", "foo"), "");
		FileSystem.File.WriteAllText(
			FileSystem.Path.Combine(path, "a", "b", "c", "foo"), "");
		FileSystem.File.WriteAllText(
			FileSystem.Path.Combine(path, "a", "b", "foo"), "");
		FileSystem.File.WriteAllText(
			FileSystem.Path.Combine(path, "a", "foo"), "");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "bar"), "");

		List<string> result = FileSystem.Directory
			.EnumerateFiles(path, "foo", enumerationOptions).ToList();

		await That(result.Count).IsEqualTo(recurseSubdirectories ? maxRecursionDepth : 0);
		if (recurseSubdirectories)
		{
			if (maxRecursionDepth > 0)
			{
				await That(result).Contains(FileSystem.Path.Combine(path, "a", "foo"));
			}

			if (maxRecursionDepth > 1)
			{
				await That(result).Contains(FileSystem.Path.Combine(path, "a", "b", "foo"));
			}

			if (maxRecursionDepth > 2)
			{
				await That(result).Contains(FileSystem.Path.Combine(path, "a", "b", "c", "foo"));
			}
		}

		await That(result).DoesNotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	[AutoArguments(true)]
	[AutoArguments(false)]
	public async Task EnumerateFiles_WithEnumerationOptions_ShouldConsiderRecurseSubdirectories(
			bool recurseSubdirectories,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			RecurseSubdirectories = recurseSubdirectories,
		};
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, "xyz"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "xyz", "foo"), "");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "bar"), "");

		List<string> result = FileSystem.Directory
			.EnumerateFiles(path, "foo", enumerationOptions).ToList();

		await That(result.Count).IsEqualTo(recurseSubdirectories ? 1 : 0);
		await That(result).DoesNotContain(FileSystem.Path.Combine(path, "xyz"));
		if (recurseSubdirectories)
		{
			await That(result).Contains(FileSystem.Path.Combine(path, "xyz", "foo"));
		}

		await That(result).DoesNotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	[AutoArguments(true)]
	[AutoArguments(false)]
	public async Task EnumerateFiles_WithEnumerationOptions_ShouldIgnoreReturnSpecialDirectories(
			bool returnSpecialDirectories,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			ReturnSpecialDirectories = returnSpecialDirectories,
		};
		FileSystem.Directory.CreateDirectory(path);
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "foo"), "");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(path, "bar"), "");

		List<string> result = FileSystem.Directory
			.EnumerateFiles(path, "*", enumerationOptions).ToList();

		await That(result.Count).IsEqualTo(2);
		await That(result).Contains(FileSystem.Path.Combine(path, "foo"));
		await That(result).Contains(FileSystem.Path.Combine(path, "bar"));
	}
#endif

	[Test]
	[AutoArguments]
	public async Task EnumerateFiles_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		void Act()
		{
			_ = FileSystem.Directory.EnumerateFiles(path, searchPattern)
				.FirstOrDefault();
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithMessageContaining(
				// The searchPattern is not included in .NET Framework
				Test.IsNetFramework ? null : $"'{searchPattern}'");
	}

	[Test]
	[AutoArguments]
	public async Task EnumerateFiles_WithoutSearchString_ShouldReturnAllFilesInDirectSubdirectories(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.EnumerateFiles(".")
			.ToList();

		await That(result.Count).IsEqualTo(2);
		await That(result).Contains(initialized[0].ToString());
		await That(result).Contains(initialized[1].ToString());
		await That(result).DoesNotContain(initialized[3].ToString());
	}

	[Test]
	[AutoArguments]
	public async Task EnumerateFiles_WithSearchPattern_ShouldReturnMatchingFiles(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.EnumerateFiles(".", initialized[0].Name)
			.ToList();

		await That(result.Count).IsEqualTo(1);
		await That(result).Contains(initialized[0].ToString());
		await That(result).DoesNotContain(initialized[1].ToString());
		await That(result).DoesNotContain(initialized[3].ToString());
	}

	[Test]
	public async Task
		EnumerateFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFilesInSubdirectories()
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
			.EnumerateFiles(".", "*.foobar", SearchOption.AllDirectories)
			.ToArray();

		await That(result).HasCount(2);
		await That(result).Contains(initialized[1].ToString());
		await That(result).Contains(initialized[3].ToString());
	}
}
