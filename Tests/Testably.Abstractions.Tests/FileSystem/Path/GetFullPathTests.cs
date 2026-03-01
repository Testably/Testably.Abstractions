namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class GetFullPathTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task GetFullPath_Dot_ShouldReturnToCurrentDirectory()
	{
		string expectedFullPath = FileSystem.Directory.GetCurrentDirectory();

		string result = FileSystem.Path.GetFullPath(".");

		await That(result).IsEqualTo(expectedFullPath);
	}

	[Test]
	[Arguments(@"C:\..", @"C:\", TestOS.Windows)]
	[Arguments(@"C:\foo", @"C:\foo", TestOS.Windows)]
	[Arguments(@"C:\foo\", @"C:\foo\", TestOS.Windows)]
	[Arguments(@"\\?\", @"\\?\", TestOS.Windows)]
	[Arguments(@"\\?\foo", @"\\?\foo", TestOS.Windows)]
	[Arguments(@"\??\", @"\??\", TestOS.Windows)]
	[Arguments(@"\??\BAR", @"\??\BAR", TestOS.Windows)]
	[Arguments("/foo", "/foo", TestOS.Linux | TestOS.Mac)]
	[Arguments("/foo/", "/foo/", TestOS.Linux | TestOS.Mac)]
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

	[Test]
	[Arguments(@"C:\..", @"C:\", TestOS.Windows)]
	[Arguments("/..", "/", TestOS.Linux | TestOS.Mac)]
	public async Task GetFullPath_ParentOfRoot_ShouldReturnRoot(string path,
		string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string result = FileSystem.Path.GetFullPath(path);

		await That(result).IsEqualTo(expected);
	}

#if FEATURE_PATH_RELATIVE
	[Test]
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
	[Test]
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
	[Test]
	[Arguments("top/../most/file", "foo/bar", "foo/bar/most/file")]
	[Arguments("top/../most/../dir/file", "foo", "foo/dir/file")]
	[Arguments("top/../../most/file", "foo/bar", "foo/most/file")]
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
	[Test]
	[Arguments(@"C:\top\..\most\file", @"C:\foo\bar", @"C:\most\file", TestOS.Windows)]
	[Arguments(@"C:\top\..\most\file", @"D:\foo\bar", @"C:\most\file", TestOS.Windows)]
	[Arguments("/top/../most/file", "/foo/bar", "/most/file", TestOS.Linux | TestOS.Mac)]
	public async Task GetFullPath_Relative_WithRootedPath_ShouldIgnoreBasePath(
		string path, string basePath, string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string result = FileSystem.Path
			.GetFullPath(path, basePath);

		await That(result).IsEqualTo(expected);
	}
#endif

	[Test]
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

	[Test]
	public async Task
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

		await That(result).IsEqualTo(expectedFullPath);
	}

	[Test]
	[Arguments(@"X:\foo/bar", @"X:\foo\bar")]
	[Arguments(@"Y:\foo/bar/", @"Y:\foo\bar\")]
	public async Task GetFullPath_ShouldFlipAltDirectorySeparators(string path,
		string expected)
	{
		Skip.IfNot(Test.RunsOnWindows);

		string result = FileSystem.Path.GetFullPath(path);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[Arguments("top/../most/file", "most/file")]
	[Arguments("top/../most/../dir/file", "dir/file")]
	[Arguments("top/../../most/file", "most/file")]
	[Arguments("top/..//../most/file", "most/file")]
	[Arguments("top//most//file", "top/most/file")]
	[Arguments("top//.//file", "top/file")]
	[Arguments("top/most/file/..", "top/most")]
	[Arguments("top/..most/file", "top/..most/file")]
	public async Task GetFullPath_ShouldRemoveRelativeSegments(string input, string expected)
	{
		string expectedRootedPath = FileTestHelper.RootDrive(Test,
			expected.Replace('/', FileSystem.Path.DirectorySeparatorChar));

		string result = FileSystem.Path.GetFullPath(FileTestHelper.RootDrive(Test, input));

		await That(result).IsEqualTo(expectedRootedPath);
	}
}
