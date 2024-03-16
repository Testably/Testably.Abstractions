namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetFullPathTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetFullPath_Dot_ShouldReturnToCurrentDirectory()
	{
		string expectedFullPath = FileSystem.Directory.GetCurrentDirectory();

		string result = FileSystem.Path.GetFullPath(".");

		result.Should().Be(expectedFullPath);
	}

	[SkippableFact]
	public void GetFullPath_RelativePathWithDrive_ShouldReturnExpectedValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string currentDirectory = FileSystem.Directory.GetCurrentDirectory();
		string drive = currentDirectory.Substring(0, 1);
		string input = $"{drive}:test.txt";
		string expectedFullPath = FileSystem.Path.Combine(currentDirectory, "test.txt");

		string result = FileSystem.Path.GetFullPath(input);

		result.Should().Be(expectedFullPath);
	}

	[SkippableFact]
	public void
		GetFullPath_RelativePathWithDrive_WhenCurrentDirectoryIsDifferent_ShouldReturnExpectedValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string currentDirectory = FileSystem.Directory.GetCurrentDirectory();
		string otherDrive = currentDirectory
			.Substring(0, 1)
			.Equals("x", StringComparison.OrdinalIgnoreCase)
			? "Y"
			: "X";
		string input = $"{otherDrive}:test.txt";
		string expectedFullPath = $@"{otherDrive}:\test.txt";

		string result = FileSystem.Path.GetFullPath(input);

		result.Should().Be(expectedFullPath);
	}

	[SkippableTheory]
	[InlineData(@"top/../most/file", @"most/file")]
	[InlineData(@"top/../most/../dir/file", @"dir/file")]
	[InlineData(@"top/../../most/file", @"most/file")]
	public void GetFullPath_ShouldNormalizeProvidedPath(string input, string expected)
	{
		string expectedRootedPath = FileTestHelper.RootDrive(Test,
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));

		string result = FileSystem.Path.GetFullPath(FileTestHelper.RootDrive(Test, input));

		result.Should().Be(expectedRootedPath);
	}

#if FEATURE_PATH_RELATIVE
	[SkippableTheory]
	[InlineData(@"top/../most/file", "foo/bar", @"foo/bar/most/file")]
	[InlineData(@"top/../most/../dir/file", "foo", @"foo/dir/file")]
	[InlineData(@"top/../../most/file", "foo/bar", @"foo/most/file")]
	public void GetFullPath_Relative_ShouldNormalizeProvidedPath(string input, string basePath,
		string expected)
	{
		string expectedRootedPath = FileTestHelper.RootDrive(Test,
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));

		string result = FileSystem.Path
			.GetFullPath(input, FileTestHelper.RootDrive(Test, basePath));

		result.Should().Be(expectedRootedPath);
	}
#endif

#if FEATURE_PATH_RELATIVE
	[SkippableFact]
	public void GetFullPath_Relative_NullBasePath_ShouldThrowArgumentNullException()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Path.GetFullPath("foo", null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("basePath");
	}
#endif
}
