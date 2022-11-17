namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetFullPathTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineData(@"top/../most/file", @"most/file")]
	[InlineData(@"top/../most/../dir/file", @"dir/file")]
	[InlineData(@"top/../../most/file", @"most/file")]
	public void GetFullPath_ShouldNormalizeProvidedPath(string input, string expected)
	{
		string expectedRootedPath = FileTestHelper.RootDrive(
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));

		string result = FileSystem.Path.GetFullPath(FileTestHelper.RootDrive(input));

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
		string expectedRootedPath = FileTestHelper.RootDrive(
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));

		string result = FileSystem.Path.GetFullPath(input, FileTestHelper.RootDrive(basePath));

		result.Should().Be(expectedRootedPath);
	}

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
