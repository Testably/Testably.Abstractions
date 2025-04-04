namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetFullPathTests
{
	[Fact]
	public void GetFullPath_Dot_ShouldReturnToCurrentDirectory()
	{
		string expectedFullPath = FileSystem.Directory.GetCurrentDirectory();

		string result = FileSystem.Path.GetFullPath(".");

		result.Should().Be(expectedFullPath);
	}

	[Theory]
	[InlineData(@"C:\..", @"C:\", TestOS.Windows)]
	[InlineData(@"C:\foo", @"C:\foo", TestOS.Windows)]
	[InlineData(@"C:\foo\", @"C:\foo\", TestOS.Windows)]
	[InlineData(@"\\?\", @"\\?\", TestOS.Windows)]
	[InlineData(@"\\?\foo", @"\\?\foo", TestOS.Windows)]
	[InlineData(@"\??\", @"\??\", TestOS.Windows)]
	[InlineData(@"\??\BAR", @"\??\BAR", TestOS.Windows)]
	[InlineData("/foo", "/foo", TestOS.Linux | TestOS.Mac)]
	[InlineData("/foo/", "/foo/", TestOS.Linux | TestOS.Mac)]
	public void GetFullPath_EdgeCases_ShouldReturnExpectedValue(
		string path, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		if (operatingSystem == TestOS.All)
		{
			expected = expected.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		}

		string result = FileSystem.Path.GetFullPath(path);

		result.Should().Be(expected);
	}

	[Theory]
	[InlineData(@"C:\..", @"C:\", TestOS.Windows)]
	[InlineData("/..", "/", TestOS.Linux | TestOS.Mac)]
	public void GetFullPath_ParentOfRoot_ShouldReturnRoot(string path,
		string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string result = FileSystem.Path.GetFullPath(path);

		result.Should().Be(expected);
	}

#if FEATURE_PATH_RELATIVE
	[Fact]
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

#if FEATURE_PATH_RELATIVE
	[Fact]
	public void GetFullPath_Relative_RelativeBasePath_ShouldThrowArgumentException()
	{
		string relativeBasePath = "not-fully-qualified-base-path";

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Path.GetFullPath("foo", relativeBasePath);
		});

		exception.Should().BeException<ArgumentException>(
			paramName: "basePath",
			messageContains: "Basepath argument is not fully qualified",
			hResult: -2147024809);
	}
#endif

#if FEATURE_PATH_RELATIVE
	[Theory]
	[InlineData("top/../most/file", "foo/bar", "foo/bar/most/file")]
	[InlineData("top/../most/../dir/file", "foo", "foo/dir/file")]
	[InlineData("top/../../most/file", "foo/bar", "foo/most/file")]
	public void GetFullPath_Relative_ShouldRemoveRelativeSegments(string input, string basePath,
		string expected)
	{
		string expectedRootedPath = FileTestHelper.RootDrive(Test,
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));
		basePath = FileTestHelper.RootDrive(Test, basePath);

		string result = FileSystem.Path
			.GetFullPath(input, basePath);

		result.Should().Be(expectedRootedPath);
	}
#endif

#if FEATURE_PATH_RELATIVE
	[Theory]
	[InlineData(@"C:\top\..\most\file", @"C:\foo\bar", @"C:\most\file", TestOS.Windows)]
	[InlineData(@"C:\top\..\most\file", @"D:\foo\bar", @"C:\most\file", TestOS.Windows)]
	[InlineData("/top/../most/file", "/foo/bar", "/most/file", TestOS.Linux | TestOS.Mac)]
	public void GetFullPath_Relative_WithRootedPath_ShouldIgnoreBasePath(
		string path, string basePath, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string result = FileSystem.Path
			.GetFullPath(path, basePath);

		result.Should().Be(expected);
	}
#endif

	[Fact]
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

	[Fact]
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

	[Theory]
	[InlineData(@"X:\foo/bar", @"X:\foo\bar")]
	[InlineData(@"Y:\foo/bar/", @"Y:\foo\bar\")]
	public void GetFullPath_ShouldFlipAltDirectorySeparators(string path,
		string expected)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string result = FileSystem.Path.GetFullPath(path);

		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("top/../most/file", "most/file")]
	[InlineData("top/../most/../dir/file", "dir/file")]
	[InlineData("top/../../most/file", "most/file")]
	[InlineData("top/..//../most/file", "most/file")]
	[InlineData("top//most//file", "top/most/file")]
	[InlineData("top//.//file", "top/file")]
	[InlineData("top/most/file/..", "top/most")]
	[InlineData("top/..most/file", "top/..most/file")]
	public void GetFullPath_ShouldRemoveRelativeSegments(string input, string expected)
	{
		string expectedRootedPath = FileTestHelper.RootDrive(Test,
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));

		string result = FileSystem.Path.GetFullPath(FileTestHelper.RootDrive(Test, input));

		result.Should().Be(expectedRootedPath);
	}
}
