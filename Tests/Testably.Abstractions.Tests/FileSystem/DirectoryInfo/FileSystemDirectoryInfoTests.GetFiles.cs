using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo(
		nameof(IFileSystem.IDirectoryInfo.GetFiles))]
	public void
		GetFiles_SearchOptionAllFiles_ShouldReturnAllFiles(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
			   .WithSubdirectory(path).Initialized(s => s
				   .WithASubdirectory().Initialized(d => d
					   .WithAFile()
					   .WithAFile())
				   .WithASubdirectory()
				   .WithAFile());
		IFileSystem.IDirectoryInfo baseDirectory =
			(IFileSystem.IDirectoryInfo)initialized[0];

		IFileSystem.IFileInfo[] result = baseDirectory
		   .GetFiles("*", SearchOption.AllDirectories);

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
	[FileSystemTests.DirectoryInfo(
		nameof(IFileSystem.IDirectoryInfo.GetFiles))]
	public void GetFiles_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
			   .WithFile(fileName)
			   .BaseDirectory;

		IFileSystem.IFileInfo[] result = baseDirectory
		   .GetFiles(searchPattern);

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
	[FileSystemTests.DirectoryInfo(
		nameof(IFileSystem.IDirectoryInfo.GetFiles))]
	public void
		GetFiles_WithEnumerationOptions_ShouldConsiderSetOptions()
	{
		IFileSystem.IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
			   .WithASubdirectory().Initialized(s => s
				   .WithFile("xyz"))
			   .WithAFile()
			   .BaseDirectory;

		IFileSystem.IFileInfo[] result = baseDirectory
		   .GetFiles("XYZ",
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System
				});

		result.Length.Should().Be(1);
		result.Should().NotContain(d => d.Name == "foo");
		result.Should().Contain(d => d.Name == "xyz");
		result.Should().NotContain(d => d.Name == "bar");
	}
#endif

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo(
		nameof(IFileSystem.IDirectoryInfo.GetFiles))]
	public void GetFiles_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		IFileSystem.IDirectoryInfo baseDirectory =
			FileSystem.DirectoryInfo.New(path);
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = baseDirectory.GetFiles(searchPattern).FirstOrDefault();
		});

		exception.Should().BeOfType<ArgumentException>();
	}

	[SkippableFact]
	[FileSystemTests.DirectoryInfo(
		nameof(IFileSystem.IDirectoryInfo.GetFiles))]
	public void
		GetFiles_WithoutSearchString_ShouldReturnAllDirectFiles()
	{
		IFileSystem.IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
			   .WithFile("foo")
			   .WithASubdirectory().Initialized(s => s
				   .WithFile("xyz"))
			   .WithFile("bar")
			   .BaseDirectory;

		IFileSystem.IFileInfo[] result = baseDirectory
		   .GetFiles();

		result.Length.Should().Be(2);
		result.Should().Contain(d => d.Name == "foo");
		result.Should().NotContain(d => d.Name == "xyz");
		result.Should().Contain(d => d.Name == "bar");
	}

	[SkippableFact]
	[FileSystemTests.DirectoryInfo(
		nameof(IFileSystem.IDirectoryInfo.GetFiles))]
	public void GetFiles_WithSearchPattern_ShouldReturnMatchingFiles()
	{
		IFileSystem.IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
			   .WithFile("foo")
			   .WithFile("bar")
			   .BaseDirectory;

		IEnumerable<IFileSystem.IFileInfo> result = baseDirectory
		   .GetFiles("foo");

		result.Should().ContainSingle(d => d.Name == "foo");
		result.Count().Should().Be(1);
	}

	[SkippableFact]
	[FileSystemTests.DirectoryInfo(
		nameof(IFileSystem.IDirectoryInfo.GetFiles))]
	public void
		GetFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFiles()
	{
		IFileSystem.IDirectoryInfo baseDirectory =
			FileSystem.Initialize()
			   .WithASubdirectory().Initialized(s => s
				   .WithFile("xyz"))
			   .WithASubdirectory().Initialized(s => s
				   .WithFile("xyz"))
			   .WithSubdirectory("xyz").Initialized(s => s
				   .WithAFile())
			   .BaseDirectory;

		IEnumerable<IFileSystem.IFileInfo> result = baseDirectory
		   .GetFiles("xyz", SearchOption.AllDirectories);

		result.Count().Should().Be(2);
	}
}