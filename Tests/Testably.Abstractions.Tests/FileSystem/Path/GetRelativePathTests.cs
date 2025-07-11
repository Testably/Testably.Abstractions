#if FEATURE_PATH_RELATIVE
using System.Linq;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetRelativePathTests
{
	[Theory]
	[AutoData]
	public async Task GetRelativePath_CommonParentDirectory_ShouldReturnRelativePath(
		string baseDirectory, string directory1, string directory2)
	{
		string path1 = FileSystem.Path.Combine(baseDirectory, directory1);
		string path2 = FileSystem.Path.Combine(baseDirectory, directory2);
		string expectedRelativePath = FileSystem.Path.Combine("..", directory2);
		string result = FileSystem.Path.GetRelativePath(path1, path2);

		await That(result).IsEqualTo(expectedRelativePath);
	}

	[Theory]
	[AutoData]
	public async Task GetRelativePath_DifferentDrives_ShouldReturnAbsolutePath(
		string path1, string path2)
	{
		Skip.IfNot(Test.RunsOnWindows, "Different drives are only supported on Windows");

		path1 = FileTestHelper.RootDrive(Test, path1, 'A');
		path2 = FileTestHelper.RootDrive(Test, path2, 'B');
		string result = FileSystem.Path.GetRelativePath(path1, path2);

		await That(result).IsEqualTo(path2);
	}

	[Theory]
	[InlineData(@"C:\FOO", @"C:\foo", ".", TestOS.Windows)]
	[InlineData("/FOO", "/foo", "../foo", TestOS.Linux)]
	[InlineData("/FOO", "/foo", ".", TestOS.Mac)]
	[InlineData("foo", "foo/", ".", TestOS.All)]
	[InlineData("foo", "foo-bar", "../foo-bar", TestOS.All)]
	[InlineData("foo/", "foo/", ".", TestOS.All)]
	[InlineData(@"C:\Foo", @"C:\Bar", @"..\Bar", TestOS.Windows)]
	[InlineData(@"C:\Foo", @"C:\Bar\", @"..\Bar\", TestOS.Windows)]
	[InlineData(@"C:\Foo", @"C:\Foo\Bar", "Bar", TestOS.Windows)]
	[InlineData(@"C:\Foo\Bar", @"C:\Foo\", "..", TestOS.Windows)]
	[InlineData(@"C:\Foo", @"C:\Foo\Bar\", @"Bar\", TestOS.Windows)]
	[InlineData(@"C:\Foo\Bar", @"C:\Bar\Bar", @"..\..\Bar\Bar", TestOS.Windows)]
	[InlineData(@"C:\Foo\Foo", @"C:\Foo\Bar", @"..\Bar", TestOS.Windows)]
	[InlineData("/Foo", "/Bar", "../Bar", TestOS.Linux | TestOS.Mac)]
	[InlineData("/", "/Bar", "Bar", TestOS.Linux | TestOS.Mac)]
	[InlineData("/Foo", "/Bar/", "../Bar/", TestOS.Linux | TestOS.Mac)]
	[InlineData("/Foo", "/Foo/Bar", "Bar", TestOS.Linux | TestOS.Mac)]
	[InlineData("/Foo", "/Foo/Bar/", "Bar/", TestOS.Linux | TestOS.Mac)]
	[InlineData("/Foo/Bar", "/Bar/Bar", "../../Bar/Bar", TestOS.Linux | TestOS.Mac)]
	[InlineData("/Foo/Foo", "/Foo/Bar", "../Bar", TestOS.Linux | TestOS.Mac)]
	public async Task GetRelativePath_EdgeCases_ShouldReturnExpectedValue(string relativeTo, string path,
		string expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));
		if (operatingSystem == TestOS.All)
		{
			expected = expected.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		}

		string result = FileSystem.Path.GetRelativePath(relativeTo, path);

		await That(result).IsEqualTo(expected);
	}

	[Fact]
	public async Task GetRelativePath_FromAbsolutePathInCurrentDirectory_ShouldReturnRelativePath()
	{
		string rootedPath = FileSystem.Path.Combine(BasePath, "input");
		FileSystem.Directory.CreateDirectory(rootedPath);
		FileSystem.Directory.SetCurrentDirectory(rootedPath);

		string result = FileSystem.Path.GetRelativePath(rootedPath, "a.txt");

		await That(result).IsEqualTo("a.txt");
	}

	[Theory]
	[AutoData]
	public async Task GetRelativePath_RootedPath_ShouldReturnAbsolutePath(
		string baseDirectory, string directory1, string directory2)
	{
		baseDirectory = FileTestHelper.RootDrive(Test, baseDirectory);
		string path1 = FileSystem.Path.Combine(baseDirectory, directory1);
		string path2 = FileSystem.Path.Combine(baseDirectory, directory2);
		string expectedRelativePath = FileSystem.Path.Combine("..", directory2);
		string result = FileSystem.Path.GetRelativePath(path1, path2);

		await That(result).IsEqualTo(expectedRelativePath);
	}

	[Fact]
	public async Task GetRelativePath_RootedPath_ShouldWorkOnAnyDrive()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string rootedPath = "/dir/subDirectory";
		FileSystem.Directory.CreateDirectory(rootedPath);
		string directory = FileSystem.Directory.GetDirectories("/dir").Single();

		string result = FileSystem.Path.GetRelativePath("/dir", directory);

		await That(result).IsEqualTo("subDirectory");
	}

	[Theory]
	[AutoData]
	public async Task GetRelativePath_ToItself_ShouldReturnDot(string path)
	{
		string expectedResult = ".";

		string result = FileSystem.Path.GetRelativePath(path, path);

		await That(result).IsEqualTo(expectedResult);
	}
}
#endif
