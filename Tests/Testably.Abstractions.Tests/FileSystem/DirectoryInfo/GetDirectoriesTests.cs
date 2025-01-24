using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class GetDirectoriesTests
{
	[Theory]
	[AutoData]
	public void
		GetDirectories_SearchOptionAllDirectories_ShouldReturnAllSubdirectories(
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
			.GetDirectories("*", SearchOption.AllDirectories);

		result.Length.Should().Be(3);
		result.Should().Contain(d => d.Name == "foo");
		result.Should().Contain(d => d.Name == "bar");
		result.Should().Contain(d => d.Name == "xyz");
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

		IDirectoryInfo[] result = baseDirectory
			.GetDirectories(searchPattern);

		if (expectToBeFound)
		{
			result.Should().ContainSingle(d => d.Name == subdirectoryName,
				$"it should match '{searchPattern}'");
		}
		else
		{
			result.Should()
				.BeEmpty($"{subdirectoryName} should not match '{searchPattern}'");
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

		IDirectoryInfo[] result = baseDirectory
			.GetDirectories("XYZ",
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System,
				});

		result.Length.Should().Be(1);
		result.Should().NotContain(d => d.Name == "foo");
		result.Should().Contain(d => d.Name == "xyz");
		result.Should().NotContain(d => d.Name == "bar");
	}
#endif

	[Theory]
	[AutoData]
	public void GetDirectories_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.DirectoryInfo.New(path);
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = baseDirectory.GetDirectories(searchPattern).FirstOrDefault();
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

		IDirectoryInfo[] result = baseDirectory
			.GetDirectories();

		result.Length.Should().Be(2);
		result.Should().Contain(d => d.Name == "foo");
		result.Should().NotContain(d => d.Name == "xyz");
		result.Should().Contain(d => d.Name == "bar");
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

		IEnumerable<IDirectoryInfo> result = baseDirectory
			.GetDirectories("foo");

		result.Should().ContainSingle(d => d.Name == "foo");
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

		IEnumerable<IDirectoryInfo> result = baseDirectory
			.GetDirectories("xyz", SearchOption.AllDirectories);

		result.Count().Should().Be(2);
	}
}
