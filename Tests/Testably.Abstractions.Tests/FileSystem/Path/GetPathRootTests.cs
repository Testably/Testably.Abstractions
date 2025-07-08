namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetPathRootTests
{
	[Theory]
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
	public async Task GetPathRoot_EdgeCases_ShouldReturnExpectedValue(
		string path, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string? result = FileSystem.Path.GetPathRoot(path);

		await That(result).IsEqualTo(expected);
	}

	[Fact]
	public async Task GetPathRoot_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetPathRoot(null);

		await That(result).IsNull();
	}

	[Theory]
	[InlineData("D:")]
	[InlineData("D:\\")]
	public async Task GetPathRoot_RootedDrive_ShouldReturnDriveOnWindows(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string? result = FileSystem.Path.GetPathRoot(path);

		await That(result).IsEqualTo(path);
	}

	[Theory]
	[InlineData("D:some-path", "D:")]
	[InlineData("D:\\some-path", "D:\\")]
	public async Task GetPathRoot_RootedDriveWithPath_ShouldReturnDriveOnWindows(
		string path, string expected)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string? result = FileSystem.Path.GetPathRoot(path);

		await That(result).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public async Task GetPathRoot_ShouldReturnDefaultValue(string path)
	{
		string? result = FileSystem.Path.GetPathRoot(path);

		await That(result).IsEqualTo("");
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task GetPathRoot_Span_ShouldReturnDefaultValue(string path)
	{
		ReadOnlySpan<char> result = FileSystem.Path.GetPathRoot(path.AsSpan());

		await That(result.ToArray()).IsEqualTo(
			System.IO.Path.GetPathRoot(path.AsSpan()).ToArray());
	}
#endif
}
