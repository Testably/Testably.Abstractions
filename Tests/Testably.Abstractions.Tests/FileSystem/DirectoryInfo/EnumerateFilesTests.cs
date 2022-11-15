using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class EnumerateFilesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFiles_SearchOptionAllFiles_ShouldReturnAllFiles(
			string path)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
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

		result.Length.Should().Be(3);
		result.Should().Contain(d => d.Name == initialized[2].Name);
		result.Should().Contain(d => d.Name == initialized[3].Name);
		result.Should().Contain(d => d.Name == initialized[5].Name);
	}

	[SkippableTheory]
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
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile(fileName)
				.BaseDirectory;

		IFileInfo[] result = baseDirectory
			.EnumerateFiles(searchPattern).ToArray();

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

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableFact]
	public void
		EnumerateFiles_WithEnumerationOptions_ShouldConsiderSetOptions()
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
					AttributesToSkip = FileAttributes.System
				}).ToArray();

		result.Length.Should().Be(1);
		result.Should().NotContain(d => d.Name == "foo");
		result.Should().Contain(d => d.Name == "xyz");
		result.Should().NotContain(d => d.Name == "bar");
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void EnumerateFiles_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		IDirectoryInfo baseDirectory =
			FileSystem.DirectoryInfo.New(path);
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = baseDirectory.EnumerateFiles(searchPattern).FirstOrDefault();
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[SkippableFact]
	public void
		EnumerateFiles_WithoutSearchString_ShouldReturnAllDirectFiles()
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

		result.Length.Should().Be(2);
		result.Should().Contain(d => d.Name == "foo");
		result.Should().NotContain(d => d.Name == "xyz");
		result.Should().Contain(d => d.Name == "bar");
	}

	[SkippableFact]
	public void EnumerateFiles_WithSearchPattern_ShouldReturnMatchingFiles()
	{
		IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
				.WithFile("foo")
				.WithFile("bar")
				.BaseDirectory;

		IEnumerable<IFileInfo> result = baseDirectory
			.EnumerateFiles("foo").ToArray();

		result.Should().ContainSingle(d => d.Name == "foo");
		result.Count().Should().Be(1);
	}

	[SkippableFact]
	public void
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

		result.Count().Should().Be(2);
	}
}
