namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetPathRootTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineData(@"\\?\foo", @"\\?\foo", TestOS.Windows)]
	[InlineData(@"\\.\BAR", @"\\.\BAR", TestOS.Windows)]
	[InlineData(@"\\.\.", @"\\.\.", TestOS.Windows)]
	[InlineData(@"\\.\\", @"\\.\", TestOS.Windows)]
	[InlineData(@"\\.\a\", @"\\.\a\", TestOS.Windows)]
	[InlineData(@"\\?\\", @"\\?\", TestOS.Windows)]
	[InlineData(@"\\?\a\", @"\\?\a\", TestOS.Windows)]
	[InlineData(@"\\?\UNC\", @"\\?\UNC\", TestOS.Windows)]
	[InlineData(@"\\?\UNC\Bar", @"\\?\UNC\Bar", TestOS.Windows)]
	[InlineData(@"\\?\UNC\a\b\c\d", @"\\?\UNC\a\b", TestOS.Windows)]
	[InlineData(@"//?/UNC/a\b\c\d", @"\\?\UNC\a\b", TestOS.Windows)]
	[InlineData(@"\\.\UNC\a\b\c\d", @"\\.\UNC\a\b", TestOS.Windows)]
	[InlineData(@"//./UNC/a\b\c\d", @"\\.\UNC\a\b", TestOS.Windows)]
	[InlineData(@"\\?\ABC\a\b\c\d", @"\\?\ABC\", TestOS.Windows)]
	[InlineData(@"//?/ABC\a\b\c\d", @"\\?\ABC\", TestOS.Windows)]
	[InlineData(@"\\.\ABC\a\b\c\d", @"\\.\ABC\", TestOS.Windows)]
	[InlineData(@"//./ABC\a\b\c\d", @"\\.\ABC\", TestOS.Windows)]
	[InlineData(@"\\X\ABC\a\b\c\d", @"\\X\ABC", TestOS.Windows)]
	[InlineData(@"//X\ABC\a\b\c\d", @"\\X\ABC", TestOS.Windows)]
	[InlineData(@"\??\ABC\a\b\c\d", @"\??\ABC\", TestOS.Windows)]
	[InlineData(@"/??/ABC\a\b\c\d", @"\", TestOS.Windows)]
	public void GetPathRoot_EdgeCases_ShouldReturnExpectedValue(
		string path, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string? result = FileSystem.Path.GetPathRoot(path);

		result.Should().Be(expected);
	}

	[SkippableFact]
	public void GetPathRoot_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetPathRoot(null);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[InlineData("D:")]
	[InlineData("D:\\")]
	public void GetPathRoot_RootedDrive_ShouldReturnDriveOnWindows(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string? result = FileSystem.Path.GetPathRoot(path);

		result.Should().Be(path);
	}

	[SkippableTheory]
	[InlineData("D:some-path", "D:")]
	[InlineData("D:\\some-path", "D:\\")]
	public void GetPathRoot_RootedDriveWithPath_ShouldReturnDriveOnWindows(
		string path, string expected)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string? result = FileSystem.Path.GetPathRoot(path);

		result.Should().Be(expected);
	}

	[SkippableTheory]
	[AutoData]
	public void GetPathRoot_ShouldReturnDefaultValue(string path)
	{
		string? result = FileSystem.Path.GetPathRoot(path);

		result.Should().Be("");
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void GetPathRoot_Span_ShouldReturnDefaultValue(string path)
	{
		ReadOnlySpan<char> result = FileSystem.Path.GetPathRoot(path.AsSpan());

		result.ToArray().Should().BeEquivalentTo(
			System.IO.Path.GetPathRoot(path.AsSpan()).ToArray());
	}
#endif
}
