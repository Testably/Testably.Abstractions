using System.Collections.Generic;
using System.IO;
using System.Linq;
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
using Testably.Abstractions.Testing.FileSystem;
#endif

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class EnumerateDirectoriesTests
{
	[Fact]
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

	[Theory]
	[AutoData]
	public void
		EnumerateDirectories_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.EnumerateDirectories(path).ToList());

		exception.Should().BeException<DirectoryNotFoundException>(
			$"'{expectedPath}'",
			hResult: -2147024893);
		FileSystem.Directory.Exists(path).Should().BeFalse();
	}

	[Fact]
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

	[Fact]
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

	[Theory]
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

	[Theory]
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
	public void EnumerateDirectories_SearchPattern_WithFileExtension_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string extension,
		string fileNameWithoutExtension)
	{
		string fileName = $"{fileNameWithoutExtension}{extension}";
		FileSystem.Initialize().WithSubdirectory(fileName);

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(".", searchPattern).ToList();

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

	[Fact]
	public async Task EnumerateDirectories_ShouldSupportExtendedLengthPaths1()
	{
		Skip.If(!Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(@"\\?\c:\bar");
		FileSystem.File.WriteAllText(@"\\?\c:\bar\foo1.txt", "foo1");
		FileSystem.File.WriteAllText(@"\\?\c:\bar\foo2.txt", "foo2");

		IEnumerable<string> result = FileSystem.Directory.EnumerateFiles(@"c:\bar");

		await That(result)
			.IsEqualTo([@"c:\bar\foo1.txt", @"c:\bar\foo2.txt"])
			.InAnyOrder();
	}

	[Fact]
	public async Task EnumerateDirectories_ShouldSupportExtendedLengthPaths2()
	{
		Skip.If(!Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(@"\\?\c:\bar");
		FileSystem.File.WriteAllText(@"\\?\c:\bar\foo1.txt", "foo1");
		FileSystem.File.WriteAllText(@"\\?\c:\bar\foo2.txt", "foo2");

		IEnumerable<string> result = FileSystem.Directory.EnumerateFiles(@"\\?\c:\bar");

		await That(result)
			.IsEqualTo([@"\\?\c:\bar\foo1.txt", @"\\?\c:\bar\foo2.txt"])
			.InAnyOrder();
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[AutoData]
	public void
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderAttributesToSkip(
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			AttributesToSkip = FileAttributes.ReadOnly,
		};
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar").Attributes = FileAttributes.ReadOnly;

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "*", enumerationOptions).ToList();

		result.Count.Should().Be(1);
		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().NotContain(FileSystem.Path.Combine(path, "bar"));
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderIgnoreInaccessible(
			bool ignoreInaccessible)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string path = @"C:\Windows\System32";
		EnumerationOptions enumerationOptions = new()
		{
			IgnoreInaccessible = ignoreInaccessible,
			RecurseSubdirectories = true,
		};
		if (FileSystem is MockFileSystem mockFileSystem)
		{
			FileSystem.Directory.CreateDirectory(
				FileSystem.Path.Combine(path, "bar"));
			FileSystem.Directory.CreateDirectory(
				FileSystem.Path.Combine(path, "foo"));
			mockFileSystem.WithAccessControlStrategy(
				new DefaultAccessControlStrategy((p, _)
					=> !p.EndsWith("foo", StringComparison.Ordinal)));
		}

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory
				.EnumerateDirectories(path, "*", enumerationOptions).ToList();
		});

		if (ignoreInaccessible)
		{
			exception.Should().BeNull();
		}
		else
		{
			exception.Should().BeException<UnauthorizedAccessException>(
				messageContains: @"Access to the path 'C:\Windows\System32\",
				hResult: -2147024891);
		}
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[InlineAutoData(MatchCasing.CaseInsensitive)]
	[InlineAutoData(MatchCasing.CaseSensitive)]
	public void
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderMatchCasing(
			MatchCasing matchCasing,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			MatchCasing = matchCasing,
		};
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "FOO", enumerationOptions).ToList();

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
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderMatchType(
			MatchType matchType,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			MatchType = matchType,
		};
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "*.", enumerationOptions).ToList();

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
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderMaxRecursionDepthWhenRecurseSubdirectoriesIsSet(
			bool recurseSubdirectories,
			int maxRecursionDepth,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			MaxRecursionDepth = maxRecursionDepth,
			RecurseSubdirectories = recurseSubdirectories,
		};
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("a/b/c/d/e/foo");
		baseDirectory.CreateSubdirectory("a/b/c/d/foo");
		baseDirectory.CreateSubdirectory("a/b/c/foo");
		baseDirectory.CreateSubdirectory("a/b/foo");
		baseDirectory.CreateSubdirectory("a/foo");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "foo", enumerationOptions).ToList();

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
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderRecurseSubdirectories(
			bool recurseSubdirectories,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			RecurseSubdirectories = recurseSubdirectories,
		};
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("xyz/foo");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "foo", enumerationOptions).ToList();

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
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderReturnSpecialDirectories(
			bool returnSpecialDirectories,
			string path)
	{
		EnumerationOptions enumerationOptions = new()
		{
			ReturnSpecialDirectories = returnSpecialDirectories,
		};
		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory(path);
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(path, "*", enumerationOptions).ToList();

		result.Count.Should().Be(returnSpecialDirectories ? 4 : 2);
		result.Should().Contain(FileSystem.Path.Combine(path, "foo"));
		result.Should().Contain(FileSystem.Path.Combine(path, "bar"));
		if (returnSpecialDirectories)
		{
			result.Should().Contain(FileSystem.Path.Combine(path, "."));
			result.Should().Contain(FileSystem.Path.Combine(path, ".."));
		}
	}
#endif

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void
		EnumerateDirectories_WithEnumerationOptions_ShouldConsiderReturnSpecialDirectoriesCorrectlyForPathRoots()
	{
		string root = FileSystem.Path.GetPathRoot(FileSystem.Directory.GetCurrentDirectory())!;
		EnumerationOptions enumerationOptions = new()
		{
			ReturnSpecialDirectories = true,
		};

		List<string> result = FileSystem.Directory
			.EnumerateDirectories(root, "*", enumerationOptions).ToList();

		if (Test.RunsOnWindows)
		{
			result.Should().NotContain(FileSystem.Path.Combine(root, "."));
			result.Should().NotContain(FileSystem.Path.Combine(root, ".."));
		}
		else
		{
			result.Should().Contain(FileSystem.Path.Combine(root, "."));
			result.Should().Contain(FileSystem.Path.Combine(root, ".."));
		}
	}
#endif

	[Theory]
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

	[Theory]
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

	[Theory]
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

	[Theory]
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

	[Fact]
	public void EnumerateDirectories_WithTrailingSlash_ShouldEnumerateSubdirectories()
	{
		string queryPath = "foo" + FileSystem.Path.DirectorySeparatorChar;
		string expectedPath = FileSystem.Path.Combine("foo", "bar");
		FileSystem.Directory.CreateDirectory(expectedPath);

		IEnumerable<string> actualResult = FileSystem.Directory.EnumerateDirectories(queryPath);

		actualResult.Should().BeEquivalentTo(expectedPath);
	}
}
