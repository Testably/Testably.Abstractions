namespace Testably.Abstractions.Tests.FileSystem.Path;

public abstract partial class FileSystemPathTests<TFileSystem>
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
}