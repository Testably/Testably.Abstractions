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
	public async Task EnumerateFileSystemInfos_SearchOptionAllFiles_ShouldReturnAllFiles(
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

		await That(result.Length).IsEqualTo(5);
		await That(result).Contains(d => d.Name == initialized[1].Name);
		await That(result).Contains(d => d.Name == initialized[2].Name);
		await That(result).Contains(d => d.Name == initialized[3].Name);
		await That(result).Contains(d => d.Name == initialized[4].Name);
		await That(result).Contains(d => d.Name == initialized[5].Name);
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
	public async Task EnumerateFileSystemInfos_SearchPattern_ShouldReturnExpectedValue(
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
			await That(result).HasSingle().Matching(d => d.Name == fileName).Because($"it should match '{searchPattern}'");
		}
		else
		{
			await That(result).IsEmpty().Because($"{fileName} should not match '{searchPattern}'");
		}
	}

	[Theory]
	[AutoData]
	public async Task EnumerateFileSystemInfos_ShouldMatchTypes(string path)
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

		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(d
			=> d.Name == initialized[1].Name && d is IDirectoryInfo);
		await That(result).Contains(d
			=> d.Name == initialized[2].Name && d is IFileInfo);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task EnumerateFileSystemInfos_WithEnumerationOptions_ShouldConsiderSetOptions()
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

		await That(result.Length).IsEqualTo(1);
		await That(result).DoesNotContain(d => d.Name == "foo");
		await That(result).Contains(d => d.Name == "xyz");
		await That(result).DoesNotContain(d => d.Name == "bar");
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
	public async Task EnumerateFileSystemInfos_WithoutSearchString_ShouldReturnAllDirectFilesAndDirectories()
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

		await That(result.Length).IsEqualTo(3);
		await That(result).Contains(d => d.Name == "foo");
		await That(result).Contains(d => d.Name == "muh");
		await That(result).DoesNotContain(d => d.Name == "xyz");
		await That(result).Contains(d => d.Name == "bar");
	}

	[Fact]
	public async Task EnumerateFileSystemInfos_WithSearchPattern_ShouldReturnMatchingFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithFile("bar")
				.BaseDirectory;

		IEnumerable<IFileSystemInfo> result = baseDirectory
			.EnumerateFileSystemInfos("foo").ToArray();

		await That(result).HasSingle().Matching(d => d.Name == "foo");
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
