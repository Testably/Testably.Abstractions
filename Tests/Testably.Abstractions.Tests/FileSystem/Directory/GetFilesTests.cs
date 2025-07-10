using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
using System.Globalization;
#endif

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class GetFilesTests
{
	[Theory]
	[AutoData]
	public async Task
		GetFiles_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, path);

		void Act()
			=> FileSystem.Directory.GetFiles(path).ToList();

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining(expectedPath).And
			.WithHResult(-2147024893);
		await That(FileSystem.Directory.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task GetFiles_Path_NotOnLinux_ShouldBeCaseInsensitive(string path)
	{
		Skip.If(Test.RunsOnLinux);

		FileSystem.Initialize()
			.WithSubdirectory(path.ToUpperInvariant()).Initialized(s => s
				.WithAFile());

		string[] result = FileSystem.Directory.GetFiles(path.ToLowerInvariant());

		await That(result.Length).IsEqualTo(1);
	}

	[Theory]
	[AutoData]
	public async Task GetFiles_Path_OnLinux_ShouldBeCaseSensitive(string path)
	{
		Skip.IfNot(Test.RunsOnLinux);

		FileSystem.Initialize()
			.WithSubdirectory(path.ToUpperInvariant()).Initialized(s => s
				.WithAFile());

		void Act()
		{
			_ = FileSystem.Directory.GetFiles(path.ToLowerInvariant());
		}

		string[] result2 = FileSystem.Directory.GetFiles(path.ToUpperInvariant());

		await That(Act).ThrowsExactly<DirectoryNotFoundException>();
		await That(result2.Length).IsEqualTo(1);
	}

	[Theory]
	[AutoData]
	public async Task GetFiles_SearchOptionAllDirectories_FullPath_ShouldReturnAllFilesWithFullPath(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(FileSystem.Directory.GetCurrentDirectory(),
				"*", SearchOption.AllDirectories)
			.ToList();

		await That(result.Count).IsEqualTo(2);
		await That(result).Contains(initialized[0].FullName);
		await That(result).Contains(initialized[2].FullName);
	}

	[Theory]
	[AutoData]
	public async Task GetFiles_SearchOptionAllDirectories_ShouldReturnAllFiles(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(".", "*", SearchOption.AllDirectories)
			.ToList();

		await That(result.Count).IsEqualTo(2);
		await That(result).Contains(initialized[0].ToString());
		await That(result).Contains(initialized[2].ToString());
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
		FileSystem.Initialize().WithFile(fileName);

		List<string> result = FileSystem.Directory
			.GetFiles(".", searchPattern).ToList();

		if (expectToBeFound)
		{
			await That(result).HasSingle().Which.EndsWith(fileName)
				.Because($"it should match {searchPattern}");
		}
		else
		{
			await That(result).IsEmpty().Because($"{fileName} should not match {searchPattern}");
		}
	}

	[Fact]
	public async Task GetFiles_SearchPatternForFileWithoutExtension_ShouldWorkConsistently()
	{
		FileSystem.Initialize()
			.WithFile("file_without_extension")
			.WithFile("file.with.an.extension");

		string[] result = FileSystem.Directory.GetFiles(".", "*.");

		await That(result.Length).IsEqualTo(1);
	}

	[Fact]
	public async Task
		GetFiles_SearchPatternWithDirectorySeparator_ShouldReturnFilesInSubdirectoryOnWindows()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(d => d
				.WithFile("bar.txt").Which(f => f.HasStringContent("inner")))
			.WithFile("bar.txt").Which(f => f.HasStringContent("outer"));

		string[] result1 = FileSystem.Directory.GetFiles(".", "foo\\*.txt");
		string[] result2 = FileSystem.Directory.GetFiles(".", ".\\*.txt");

		if (Test.RunsOnWindows)
		{
			await That(result1.Length).IsEqualTo(1);
			await That(FileSystem.File.ReadAllText(result1[0])).IsEqualTo("inner");
			await That(result2.Length).IsEqualTo(1);
			await That(FileSystem.File.ReadAllText(result2[0])).IsEqualTo("outer");
		}
		else
		{
			await That(result1).IsEmpty();
			await That(result2).IsEmpty();
		}
	}

	[Fact]
	public async Task GetFiles_SearchPatternWithTooManyAsterisk_ShouldWorkConsistently()
	{
		FileSystem.Initialize()
			.WithFile("result.test.001.txt");

		string[] result = FileSystem.Directory.GetFiles(".", "*.test.*.*.*.*");

		await That(result.Length).IsEqualTo(1);
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Theory]
	[AutoData]
	public async Task GetFiles_WithEnumerationOptions_ShouldConsiderSetOptions(
			string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(".",
				initialized[2].Name.ToUpper(CultureInfo.InvariantCulture),
				new EnumerationOptions
				{
					MatchCasing = MatchCasing.CaseInsensitive,
					RecurseSubdirectories = true,
					// Filename could start with a leading '.' indicating it as Hidden in Linux
					AttributesToSkip = FileAttributes.System,
				}).ToList();

		await That(result.Count).IsEqualTo(1);
		await That(result).DoesNotContain(initialized[0].ToString());
		await That(result).Contains(initialized[2].ToString());
	}
#endif

	[Theory]
	[AutoData]
	public async Task GetFiles_WithNewline_ShouldThrowArgumentException(
		string path)
	{
		string searchPattern = "foo\0bar";

		void Act()
		{
			_ = FileSystem.Directory.GetFiles(path, searchPattern)
				.FirstOrDefault();
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithMessageContaining(
				// The searchPattern is not included in .NET Framework
				Test.IsNetFramework ? null : $"'{searchPattern}'");
	}

	[Theory]
	[AutoData]
	public async Task GetFiles_WithoutSearchString_ShouldReturnAllFilesInDirectSubdirectories(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(".")
			.ToList();

		await That(result.Count).IsEqualTo(2);
		await That(result).Contains(initialized[0].ToString());
		await That(result).Contains(initialized[1].ToString());
		await That(result).DoesNotContain(initialized[3].ToString());
	}

	[Theory]
	[AutoData]
	public async Task GetFiles_WithRelativePathAndSubfolders_ShouldReturnRelativeFilePath(
		string subfolder1, string subfolder2, string[] files)
	{
		string workingDirectory = FileSystem.Path.Combine(BasePath, subfolder1, subfolder2);
		FileSystem.Directory.CreateDirectory(workingDirectory);
		foreach (string file in files)
		{
			FileSystem.File.Create(FileSystem.Path.Combine(workingDirectory, file));
		}

		FileSystem.Directory.SetCurrentDirectory(subfolder1);
		IEnumerable<string> expectation =
			files.Select(f => FileSystem.Path.Combine("..", subfolder1, subfolder2, f));
		string path = FileSystem.Path.Combine("..", subfolder1, subfolder2);

		List<string> result = FileSystem.Directory.GetFiles(path).ToList();

		await That(result).IsEqualTo(expectation).InAnyOrder();
	}

	[Theory]
	[AutoData]
	public async Task GetFiles_WithSearchPattern_ShouldReturnMatchingFiles(
		string path)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.InitializeIn(path)
				.WithAFile()
				.WithAFile()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		List<string> result = FileSystem.Directory
			.GetFiles(".", initialized[0].Name)
			.ToList();

		await That(result.Count).IsEqualTo(1);
		await That(result).Contains(initialized[0].ToString());
		await That(result).DoesNotContain(initialized[1].ToString());
		await That(result).DoesNotContain(initialized[3].ToString());
	}

	[Fact]
	public async Task
		GetFiles_WithSearchPatternInSubdirectory_ShouldReturnMatchingFilesInSubdirectories()
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithAFile("foobar"))
				.WithASubdirectory().Initialized(s => s
					.WithAFile("foobar"))
				.WithASubdirectory().Initialized(s => s
					.WithAFile());

		IEnumerable<string> result = FileSystem.Directory
			.GetFiles(".", "*.foobar", SearchOption.AllDirectories)
			.ToArray();

		await That(result).HasCount(2);
		await That(result).Contains(initialized[1].ToString());
		await That(result).Contains(initialized[3].ToString());
	}
}
