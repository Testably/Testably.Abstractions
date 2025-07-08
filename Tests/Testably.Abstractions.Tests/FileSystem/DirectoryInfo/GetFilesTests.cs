using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class GetFilesTests
{
	[Theory]
	[AutoData]
	public async Task GetFiles_SearchOptionAllFiles_ShouldReturnAllFiles(
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
			.GetFiles("*", SearchOption.AllDirectories);

		await That(result.Length).IsEqualTo(3);
		await That(result).Contains(d => d.Name == initialized[2].Name);
		await That(result).Contains(d => d.Name == initialized[3].Name);
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
	public async Task GetFiles_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile(fileName)
				.BaseDirectory;

		IFileInfo[] result = baseDirectory
			.GetFiles(searchPattern);

		if (expectToBeFound)
		{
			await That(result).HasSingle().Matching(d => d.Name == fileName).Because($"it should match '{searchPattern}'");
		}
		else
		{
			await That(result).IsEmpty().Because($"{fileName} should not match '{searchPattern}'");
		}
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Fact]
	public async Task GetFiles_WithEnumerationOptions_ShouldConsiderSetOptions()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithFile("xyz"))
				.WithAFile()
				.BaseDirectory;

		IFileInfo[] result = baseDirectory
			.GetFiles("XYZ",
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
	public void GetFiles_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.DirectoryInfo.New(path);
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = baseDirectory.GetFiles(searchPattern).FirstOrDefault();
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[Fact]
	public async Task GetFiles_WithoutSearchString_ShouldReturnAllDirectFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithASubdirectory().Initialized(s => s
					.WithFile("xyz"))
				.WithFile("bar")
				.BaseDirectory;

		IFileInfo[] result = baseDirectory
			.GetFiles();

		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(d => d.Name == "foo");
		await That(result).DoesNotContain(d => d.Name == "xyz");
		await That(result).Contains(d => d.Name == "bar");
	}

	[Fact]
	public async Task GetFiles_WithSearchPattern_ShouldReturnMatchingFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithFile("bar")
				.BaseDirectory;

		IEnumerable<IFileInfo> result = baseDirectory
			.GetFiles("foo");

		await That(result).HasSingle().Matching(d => d.Name == "foo");
		result.Count().Should().Be(1);
	}

	[Fact]
	public void
		GetFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFiles()
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
			.GetFiles("xyz", SearchOption.AllDirectories);

		await That(result).HasCount(2);
	}
}
