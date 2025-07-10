using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetFullPathTests
{
	[Fact]
	public async Task GetFullPath_Dot_ShouldReturnToCurrentDirectory()
	{
		string expectedFullPath = FileSystem.Directory.GetCurrentDirectory();

		string result = FileSystem.Path.GetFullPath(".");

		await That(result).IsEqualTo(expectedFullPath);
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
	public async Task GetFullPath_EdgeCases_ShouldReturnExpectedValue(
		string path, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		if (operatingSystem == TestOS.All)
		{
			expected = expected.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		}

		string result = FileSystem.Path.GetFullPath(path);

		await That(result).IsEqualTo(expected);
	}

	[Theory]
	[InlineData(@"C:\..", @"C:\", TestOS.Windows)]
	[InlineData("/..", "/", TestOS.Linux | TestOS.Mac)]
	public async Task GetFullPath_ParentOfRoot_ShouldReturnRoot(string path,
		string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string result = FileSystem.Path.GetFullPath(path);

		await That(result).IsEqualTo(expected);
	}

#if FEATURE_PATH_RELATIVE
	[Fact]
	public async Task GetFullPath_Relative_NullBasePath_ShouldThrowArgumentNullException()
	{
		void Act()
		{
			FileSystem.Path.GetFullPath("foo", null!);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().Whose(x => x.ParamName, it => it.IsEqualTo("basePath"));
	}
#endif

#if FEATURE_PATH_RELATIVE
	[Fact]
	public async Task GetFullPath_Relative_RelativeBasePath_ShouldThrowArgumentException()
	{
		string relativeBasePath = "not-fully-qualified-base-path";

		void Act()
		{
			FileSystem.Path.GetFullPath("foo", relativeBasePath);
		}

		await That(Act).Throws<ArgumentException>()
			.WithParamName("basePath").And
			.WithHResult(-2147024809).And
			.WithMessage("Basepath argument is not fully qualified").AsPrefix();
	}
#endif

#if FEATURE_PATH_RELATIVE
	[Theory]
	[InlineData("top/../most/file", "foo/bar", "foo/bar/most/file")]
	[InlineData("top/../most/../dir/file", "foo", "foo/dir/file")]
	[InlineData("top/../../most/file", "foo/bar", "foo/most/file")]
	public async Task GetFullPath_Relative_ShouldRemoveRelativeSegments(string input, string basePath,
		string expected)
	{
		string expectedRootedPath = FileTestHelper.RootDrive(Test,
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));
		basePath = FileTestHelper.RootDrive(Test, basePath);

		string result = FileSystem.Path
			.GetFullPath(input, basePath);

		await That(result).IsEqualTo(expectedRootedPath);
	}
#endif

#if FEATURE_PATH_RELATIVE
	[Theory]
	[InlineData(@"C:\top\..\most\file", @"C:\foo\bar", @"C:\most\file", TestOS.Windows)]
	[InlineData(@"C:\top\..\most\file", @"D:\foo\bar", @"C:\most\file", TestOS.Windows)]
	[InlineData("/top/../most/file", "/foo/bar", "/most/file", TestOS.Linux | TestOS.Mac)]
	public async Task GetFullPath_Relative_WithRootedPath_ShouldIgnoreBasePath(
		string path, string basePath, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string result = FileSystem.Path
			.GetFullPath(path, basePath);

		await That(result).IsEqualTo(expected);
	}
#endif

	[Fact]
	public async Task GetFullPath_RelativePathWithDrive_ShouldReturnExpectedValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string currentDirectory = FileSystem.Directory.GetCurrentDirectory();
		string drive = currentDirectory.Substring(0, 1);
		string input = $"{drive}:test.txt";
		string expectedFullPath = FileSystem.Path.Combine(currentDirectory, "test.txt");

		string result = FileSystem.Path.GetFullPath(input);

		await That(result).IsEqualTo(expectedFullPath);
	}

	[Fact]
	public async Task GetFullPath_RelativePathWithDrive_WhenCurrentDirectoryIsDifferent_ShouldReturnExpectedValue()
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

		await That(result).IsEqualTo(expectedFullPath);
	}

	[Theory]
	[InlineData(@"X:\foo/bar", @"X:\foo\bar")]
	[InlineData(@"Y:\foo/bar/", @"Y:\foo\bar\")]
	public async Task GetFullPath_ShouldFlipAltDirectorySeparators(string path,
		string expected)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string result = FileSystem.Path.GetFullPath(path);

		await That(result).IsEqualTo(expected);
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
	public async Task GetFullPath_ShouldRemoveRelativeSegments(string input, string expected)
	{
		string expectedRootedPath = FileTestHelper.RootDrive(Test,
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));

		string result = FileSystem.Path.GetFullPath(FileTestHelper.RootDrive(Test, input));

		await That(result).IsEqualTo(expectedRootedPath);
	}
}
