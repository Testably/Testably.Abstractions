using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFileSystemEntries_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = System.IO.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.EnumerateFileSystemEntries(path).ToList());

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.Message.Should().Contain($"'{expectedPath}'.");
		FileSystem.Directory.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFileSystemEntries_SearchOptionAllDirectories_FullPath_ShouldReturnAllFileSystemEntriesWithFullPath(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFileSystemEntries(
				FileSystem.Path.GetFullPath(path),
				"*",
				SearchOption.AllDirectories)
		   .ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(initialized[0].FullName);
		result.Should().Contain(initialized[1].FullName);
		result.Should().Contain(initialized[2].FullName);
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFileSystemEntries_SearchOptionAllDirectories_ShouldReturnAllFileSystemEntries(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories)
		   .ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[1].ToString());
		result.Should().Contain(initialized[2].ToString());
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
	public void EnumerateFileSystemEntries_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
		   .EnumerateFileSystemEntries(".", searchPattern).ToList();

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

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFileSystemEntries_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFileSystemEntries(path, initialized[2].Name.ToUpper(),
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System
				}).ToList();

		result.Count.Should().Be(1, $"{initialized[2]} should be found.");
		result.Should().NotContain(initialized[0].ToString());
		result.Should().Contain(initialized[2].ToString());
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void EnumerateFileSystemEntries_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.EnumerateFileSystemEntries(path, searchPattern)
			   .FirstOrDefault();
		});

#if NETFRAMEWORK
		// The searchPattern is not included in .NET Framework
		exception.Should().BeOfType<ArgumentException>();
#else
		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain($"'{searchPattern}'");
#endif
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFileSystemEntries_WithoutSearchString_ShouldReturnAllFileSystemEntriesInDirectSubdirectories(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result =
			FileSystem.Directory.EnumerateFileSystemEntries(path).ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[1].ToString());
		result.Should().Contain(initialized[2].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[SkippableTheory]
	[AutoData]
	public void
		EnumerateFileSystemEntries_WithSearchPattern_ShouldReturnMatchingFileSystemEntries(
			string path)
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .EnumerateFileSystemEntries(path, initialized[0].Name)
		   .ToList();

		result.Count.Should().Be(1);
		result.Should().Contain(initialized[0].ToString());
		result.Should().NotContain(initialized[1].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[SkippableFact]
	public void
		EnumerateFileSystemEntries_WithSearchPatternInSubdirectory_ShouldReturnMatchingFileSystemEntriesInSubdirectories()
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile("foobar"))
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile("foobar"))
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		IEnumerable<string> result = FileSystem.Directory
		   .EnumerateFileSystemEntries(".", "*.foobar", SearchOption.AllDirectories)
		   .ToArray();

		result.Count().Should().Be(2);
		result.Should().Contain(initialized[1].ToString());
		result.Should().Contain(initialized[3].ToString());
	}
}