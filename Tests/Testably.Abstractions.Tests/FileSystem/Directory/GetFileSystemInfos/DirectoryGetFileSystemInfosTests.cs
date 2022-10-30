using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Tests.FileSystem.Directory.GetFileSystemInfos;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DirectoryGetFileSystemInfosTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
		GetFileSystemEntries_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = System.IO.Path.Combine(BasePath, path);
		Exception? exception =
			Record.Exception(()
				=> FileSystem.Directory.GetFileSystemEntries(path).ToList());

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.Message.Should().Contain($"'{expectedPath}'.");
		FileSystem.Directory.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void
		GetFileSystemEntries_SearchOptionAllDirectories_FullPath_ShouldReturnAllFileSystemEntriesWithFullPath(
			string path)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .GetFileSystemEntries(
				FileSystem.Directory.GetCurrentDirectory(),
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
		GetFileSystemEntries_SearchOptionAllDirectories_ShouldReturnAllFileSystemEntries(
			string path)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .GetFileSystemEntries(".", "*", SearchOption.AllDirectories)
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
	public void GetFileSystemEntries_SearchPattern_ShouldReturnExpectedValue(
		bool expectToBeFound, string searchPattern, string fileName)
	{
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
		   .GetFileSystemEntries(".", searchPattern).ToList();

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
		GetFileSystemEntries_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .GetFileSystemEntries(".",
				initialized[2].Name.ToUpper(),
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
	public void GetFileSystemEntries_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.GetFileSystemEntries(path, searchPattern)
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
		GetFileSystemEntries_WithoutSearchString_ShouldReturnAllFileSystemEntriesInDirectSubdirectories(
			string path)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result =
			FileSystem.Directory
			   .GetFileSystemEntries(".")
			   .ToList();

		result.Count.Should().Be(3);
		result.Should().Contain(initialized[0].ToString());
		result.Should().Contain(initialized[1].ToString());
		result.Should().Contain(initialized[2].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[SkippableTheory]
	[AutoData]
	public void
		GetFileSystemEntries_WithSearchPattern_ShouldReturnMatchingFileSystemEntries(
			string path)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.InitializeIn(path)
			   .WithAFile()
			   .WithAFile()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		List<string> result = FileSystem.Directory
		   .GetFileSystemEntries(".", initialized[0].Name)
		   .ToList();

		result.Count.Should().Be(1);
		result.Should().Contain(initialized[0].ToString());
		result.Should().NotContain(initialized[1].ToString());
		result.Should().NotContain(initialized[3].ToString());
	}

	[SkippableFact]
	public void
		GetFileSystemEntries_WithSearchPatternInSubdirectory_ShouldReturnMatchingFileSystemEntriesInSubdirectories()
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile("foobar"))
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile("foobar"))
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());

		IEnumerable<string> result = FileSystem.Directory
		   .GetFileSystemEntries(".", "*.foobar", SearchOption.AllDirectories)
		   .ToArray();

		result.Count().Should().Be(2);
		result.Should().Contain(initialized[1].ToString());
		result.Should().Contain(initialized[3].ToString());
	}
}