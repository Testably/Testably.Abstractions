using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class GetFileSystemInfosTests
{
	[Theory]
	[AutoData]
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
			await That(result).HasSingle().Matching(d => d.Name == fileName)
				.Because($"it should match '{searchPattern}'");
		}
		else
		{
			await That(result).IsEmpty().Because($"{fileName} should not match '{searchPattern}'");
		}
	}

	[Theory]
	[AutoData]
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
			=> d.Name == initialized[1].Name && d is IDirectoryInfo);
		await That(result).Contains(d
			=> d.Name == initialized[2].Name && d is IFileInfo);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
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
		await That(result).DoesNotContain(d => d.Name == "foo");
		await That(result).Contains(d => d.Name == "xyz");
		await That(result).DoesNotContain(d => d.Name == "bar");
	}
#endif

	[Theory]
	[AutoData]
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

	[Fact]
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
		await That(result).Contains(d => d.Name == "foo");
		await That(result).Contains(d => d.Name == "muh");
		await That(result).DoesNotContain(d => d.Name == "xyz");
		await That(result).Contains(d => d.Name == "bar");
	}

	[Fact]
	public async Task GetFileSystemInfos_WithSearchPattern_ShouldReturnMatchingFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithFile("bar")
				.BaseDirectory;

		IEnumerable<IFileSystemInfo> result = baseDirectory
			.GetFileSystemInfos("foo");

		await That(result).HasSingle().Matching(d => d.Name == "foo");
		await That(result.Count()).IsEqualTo(1);
	}

	[Fact]
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
