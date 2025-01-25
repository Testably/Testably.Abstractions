using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class EnumerateFilesTests
{
	[Theory]
	[AutoData]
	public void
		EnumerateFiles_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.EnumerateFiles(path).ToList());

		exception.Should().BeException<DirectoryNotFoundException>(
			$"'{expectedPath}'",
			hResult: -2147024893);
		FileSystem.Should().NotHaveDirectory(path);
	}

	[Theory]
	[AutoData]
	public void
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

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].FullName);
		result.Should().Contain(initialized[2].FullName);
	}

	[Theory]
	[AutoData]
	public void EnumerateFiles_SearchOptionAllDirectories_ShouldReturnAllFiles(
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

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[2].ToString());
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

	[Theory]
	[InlineAutoData(true, "*.xls", ".xls")]
	[InlineAutoData(false, "*.x", ".xls")]
#if NETFRAMEWORK
	[InlineAutoData(true, "*.xls", ".xlsx")]
#else
	[InlineAutoData(false, "*.xls", ".xlsx")]
#endif
	[InlineAutoData(false, "foo.x", ".xls", "foo")]
	[InlineAutoData(false, "?.xls", ".xlsx", "a")]
	public void EnumerateFiles_SearchPattern_WithFileExtension_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string extension,
		string fileNameWithoutExtension)
	{
		string fileName = $"{fileNameWithoutExtension}{extension}";
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
			.EnumerateFiles(".", searchPattern).ToList();

		if (expectToBeFound)
		{
			result.Should().ContainSingle(
				extension,
				$"it should match {searchPattern}");
		}
		else
		{
			result.Should()
				.BeEmpty($"{extension} should not match {searchPattern}");
		}
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[AutoData]
	public void
		EnumerateFiles_WithEnumerationOptions_ShouldConsiderAttributesToSkip(
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

		result.Count.Should().Be(1);
		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().NotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[InlineAutoData(MatchCasing.CaseInsensitive)]
	[InlineAutoData(MatchCasing.CaseSensitive)]
	public void
		EnumerateFiles_WithEnumerationOptions_ShouldConsiderMatchCasing(
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

		result.Count.Should().Be(matchCasing == MatchCasing.CaseInsensitive ? 1 : 0);
		if (matchCasing == MatchCasing.CaseInsensitive)
		{
			result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		}

		result.Should().NotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[InlineAutoData(MatchType.Simple)]
	[InlineAutoData(MatchType.Win32)]
	public void
		EnumerateFiles_WithEnumerationOptions_ShouldConsiderMatchType(
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

		result.Count.Should().Be(matchType == MatchType.Win32 ? 2 : 0);
		if (matchType == MatchType.Win32)
		{
			result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
			result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
		}
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[InlineAutoData(true, 0)]
	[InlineAutoData(true, 1)]
	[InlineAutoData(true, 2)]
	[InlineAutoData(true, 3)]
	[InlineAutoData(false, 2)]
	public void
		EnumerateFiles_WithEnumerationOptions_ShouldConsiderMaxRecursionDepthWhenRecurseSubdirectoriesIsSet(
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

		result.Count.Should().Be(recurseSubdirectories ? maxRecursionDepth : 0);
		if (recurseSubdirectories)
		{
			if (maxRecursionDepth > 0)
			{
				result.Should().Contain(FileSystem.Path.Combine(path, "a", "foo"));
			}

			if (maxRecursionDepth > 1)
			{
				result.Should().Contain(FileSystem.Path.Combine(path, "a", "b", "foo"));
			}

			if (maxRecursionDepth > 2)
			{
				result.Should().Contain(FileSystem.Path.Combine(path, "a", "b", "c", "foo"));
			}
		}

		result.Should().NotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public void
		EnumerateFiles_WithEnumerationOptions_ShouldConsiderRecurseSubdirectories(
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

		result.Count.Should().Be(recurseSubdirectories ? 1 : 0);
		result.Should().NotContain(FileSystem.Path.Combine(path, "xyz"));
		if (recurseSubdirectories)
		{
			result.Should().Contain(FileSystem.Path.Combine(path, "xyz", "foo"));
		}

		result.Should().NotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public void
		EnumerateFiles_WithEnumerationOptions_ShouldIgnoreReturnSpecialDirectories(
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

		result.Count.Should().Be(2);
		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

	[Theory]
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

		exception.Should().BeException<ArgumentException>(hResult: -2147024809,
			// The searchPattern is not included in .NET Framework
			messageContains: Test.IsNetFramework ? null : $"'{searchPattern}'");
	}

	[Theory]
	[AutoData]
	public void
		EnumerateFiles_WithoutSearchString_ShouldReturnAllFilesInDirectSubdirectories(
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

		result.Count.Should().Be(2);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[1].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[Theory]
	[AutoData]
	public void EnumerateFiles_WithSearchPattern_ShouldReturnMatchingFiles(
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

		result.Count.Should().Be(1);
		result.Should().Contain(initialized[0].ToString());
		result.Should().NotContain(initialized[1].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[Fact]
	public void
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

		result.Count().Should().Be(2);
		result.Should().Contain(initialized[1].ToString());
		result.Should().Contain(initialized[3].ToString());
	}
}
