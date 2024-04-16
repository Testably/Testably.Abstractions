namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class IsPathRootedTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineData("/foo", TestOs.All)]
	[InlineData(@"\foo", TestOs.Windows)]
	[InlineData("foo/bar", TestOs.None)]
	public void IsPathRooted_ShouldReturnDefaultValue(string path, TestOs isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path);

		result.Should().Be(Test.RunsOn(isRootedOn));
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[InlineData("/foo", TestOs.All)]
	[InlineData(@"\foo", TestOs.Windows)]
	[InlineData("foo/bar", TestOs.None)]
	public void IsPathRooted_Span_ShouldReturnDefaultValue(string path, TestOs isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path.AsSpan());

		result.Should().Be(Test.RunsOn(isRootedOn));
	}
#endif
}
