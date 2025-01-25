using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class EnumerateFileSystemInfosTests
{
	[Theory]
	[AutoData]
	public void
		EnumerateFileSystemInfos_SearchOptionAllFiles_ShouldReturnAllFiles(
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
			.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).ToArray();

		result.Length.Should().Be(5);
		result.Should().Contain(d => d.Name == initialized[1].Name);
		result.Should().Contain(d => d.Name == initialized[2].Name);
		result.Should().Contain(d => d.Name == initialized[3].Name);
		result.Should().Contain(d => d.Name == initialized[4].Name);
		result.Should().Contain(d => d.Name == initialized[5].Name);
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
	public void EnumerateFileSystemInfos_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile(fileName)
				.BaseDirectory;

		IFileSystemInfo[] result = baseDirectory
			.EnumerateFileSystemInfos(searchPattern).ToArray();

		if (expectToBeFound)
		{
			result.Should().ContainSingle(d => d.Name == fileName,
				$"it should match '{searchPattern}'");
		}
		else
		{
			result.Should()
				.BeEmpty($"{fileName} should not match '{searchPattern}'");
		}
	}

	[Theory]
	[AutoData]
	public void
		EnumerateFileSystemInfos_ShouldMatchTypes(string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(path).Initialized(s => s
					.WithASubdirectory()
					.WithAFile());
		IDirectoryInfo baseDirectory =
			(IDirectoryInfo)initialized[0];

		IFileSystemInfo[] result = baseDirectory
			.EnumerateFileSystemInfos("*").ToArray();

		result.Length.Should().Be(2);
		result.Should().Contain(d
			=> d.Name == initialized[1].Name && d is IDirectoryInfo);
		result.Should().Contain(d
			=> d.Name == initialized[2].Name && d is IFileInfo);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public void
		EnumerateFileSystemInfos_WithEnumerationOptions_ShouldConsiderSetOptions()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithFile("xyz"))
				.WithAFile()
				.BaseDirectory;

		IFileSystemInfo[] result = baseDirectory
			.EnumerateFileSystemInfos("XYZ",
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System,
				}).ToArray();

		result.Length.Should().Be(1);
		result.Should().NotContain(d => d.Name == "foo");
		result.Should().Contain(d => d.Name == "xyz");
		result.Should().NotContain(d => d.Name == "bar");
	}
#endif

	[Theory]
	[AutoData]
	public void EnumerateFileSystemInfos_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.DirectoryInfo.New(path);
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = baseDirectory.EnumerateFileSystemInfos(searchPattern).FirstOrDefault();
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[Fact]
	public void
		EnumerateFileSystemInfos_WithoutSearchString_ShouldReturnAllDirectFilesAndDirectories()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithSubdirectory("muh").Initialized(s => s
					.WithFile("xyz"))
				.WithFile("bar")
				.BaseDirectory;

		IFileSystemInfo[] result = baseDirectory
			.EnumerateFileSystemInfos().ToArray();

		result.Length.Should().Be(3);
		result.Should().Contain(d => d.Name == "foo");
		result.Should().Contain(d => d.Name == "muh");
		result.Should().NotContain(d => d.Name == "xyz");
		result.Should().Contain(d => d.Name == "bar");
	}

	[Fact]
	public void EnumerateFileSystemInfos_WithSearchPattern_ShouldReturnMatchingFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithFile("bar")
				.BaseDirectory;

		IEnumerable<IFileSystemInfo> result = baseDirectory
			.EnumerateFileSystemInfos("foo").ToArray();

		result.Should().ContainSingle(d => d.Name == "foo");
		result.Count().Should().Be(1);
	}

	[Fact]
	public void
		EnumerateFileSystemInfos_WithSearchPatternInSubdirectory_ShouldReturnMatchingFiles()
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
			.EnumerateFileSystemInfos("xyz", SearchOption.AllDirectories);

		result.Count().Should().Be(3);
	}
}
