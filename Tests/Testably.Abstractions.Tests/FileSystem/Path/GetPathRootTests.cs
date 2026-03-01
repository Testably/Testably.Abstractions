namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class GetPathRootTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[Arguments(@"\\?\foo", @"\\?\foo", TestOS.Windows)]
	[Arguments(@"\\.\BAR", @"\\.\BAR", TestOS.Windows)]
	[Arguments(@"\\.\.", @"\\.\.", TestOS.Windows)]
	[Arguments(@"\\.\\", @"\\.\", TestOS.Windows)]
	[Arguments(@"\\.\a\", @"\\.\a\", TestOS.Windows)]
	[Arguments(@"\\?\\", @"\\?\", TestOS.Windows)]
	[Arguments(@"\\?\a\", @"\\?\a\", TestOS.Windows)]
	[Arguments(@"\\?\UNC\", @"\\?\UNC\", TestOS.Windows)]
	[Arguments(@"\\?\UNC\Bar", @"\\?\UNC\Bar", TestOS.Windows)]
	[Arguments(@"\\?\UNC\a\b\c\d", @"\\?\UNC\a\b", TestOS.Windows)]
	[Arguments(@"//?/UNC/a\b\c\d", @"\\?\UNC\a\b", TestOS.Windows)]
	[Arguments(@"\\.\UNC\a\b\c\d", @"\\.\UNC\a\b", TestOS.Windows)]
	[Arguments(@"//./UNC/a\b\c\d", @"\\.\UNC\a\b", TestOS.Windows)]
	[Arguments(@"\\?\ABC\a\b\c\d", @"\\?\ABC\", TestOS.Windows)]
	[Arguments(@"//?/ABC\a\b\c\d", @"\\?\ABC\", TestOS.Windows)]
	[Arguments(@"\\.\ABC\a\b\c\d", @"\\.\ABC\", TestOS.Windows)]
	[Arguments(@"//./ABC\a\b\c\d", @"\\.\ABC\", TestOS.Windows)]
	[Arguments(@"\\X\ABC\a\b\c\d", @"\\X\ABC", TestOS.Windows)]
	[Arguments(@"//X\ABC\a\b\c\d", @"\\X\ABC", TestOS.Windows)]
	[Arguments(@"\??\ABC\a\b\c\d", @"\??\ABC\", TestOS.Windows)]
	[Arguments(@"/??/ABC\a\b\c\d", @"\", TestOS.Windows)]
	public async Task GetPathRoot_EdgeCases_ShouldReturnExpectedValue(
		string path, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string? result = FileSystem.Path.GetPathRoot(path);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	public async Task GetPathRoot_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetPathRoot(null);

		await That(result).IsNull();
	}

	[Test]
	[Arguments("D:")]
	[Arguments("D:\\")]
	public async Task GetPathRoot_RootedDrive_ShouldReturnDriveOnWindows(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string? result = FileSystem.Path.GetPathRoot(path);

		await That(result).IsEqualTo(path);
	}

	[Test]
	[Arguments("D:some-path", "D:")]
	[Arguments("D:\\some-path", "D:\\")]
	public async Task GetPathRoot_RootedDriveWithPath_ShouldReturnDriveOnWindows(
		string path, string expected)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string? result = FileSystem.Path.GetPathRoot(path);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[AutoArguments]
	public async Task GetPathRoot_ShouldReturnDefaultValue(string path)
	{
		string? result = FileSystem.Path.GetPathRoot(path);

		await That(result).IsEqualTo("");
	}

#if FEATURE_SPAN
	[Test]
	[AutoArguments]
	public async Task GetPathRoot_Span_ShouldReturnDefaultValue(string path)
	{
		ReadOnlySpan<char> result = FileSystem.Path.GetPathRoot(path.AsSpan());

		await That(result.ToArray()).IsEqualTo(
			System.IO.Path.GetPathRoot(path.AsSpan()).ToArray());
	}
#endif
}
