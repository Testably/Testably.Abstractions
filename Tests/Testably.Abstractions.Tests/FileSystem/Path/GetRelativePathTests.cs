#if FEATURE_PATH_RELATIVE
using System.Linq;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class GetRelativePathTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task GetRelativePath_CommonParentDirectory_ShouldReturnRelativePath(
		string baseDirectory, string directory1, string directory2)
	{
		string path1 = FileSystem.Path.Combine(baseDirectory, directory1);
		string path2 = FileSystem.Path.Combine(baseDirectory, directory2);
		string expectedRelativePath = FileSystem.Path.Combine("..", directory2);
		string result = FileSystem.Path.GetRelativePath(path1, path2);

		await That(result).IsEqualTo(expectedRelativePath);
	}

	[Test]
	[AutoArguments]
	public async Task GetRelativePath_DifferentDrives_ShouldReturnAbsolutePath(
		string path1, string path2)
	{
		Skip.IfNot(Test.RunsOnWindows, "Different drives are only supported on Windows");

		path1 = FileTestHelper.RootDrive(Test, path1, 'A');
		path2 = FileTestHelper.RootDrive(Test, path2, 'B');
		string result = FileSystem.Path.GetRelativePath(path1, path2);

		await That(result).IsEqualTo(path2);
	}

	[Test]
	[Arguments(@"C:\FOO", @"C:\foo", ".", TestOS.Windows)]
	[Arguments("/FOO", "/foo", "../foo", TestOS.Linux)]
	[Arguments("/FOO", "/foo", ".", TestOS.Mac)]
	[Arguments("foo", "foo/", ".", TestOS.All)]
	[Arguments("foo", "foo-bar", "../foo-bar", TestOS.All)]
	[Arguments("foo/", "foo/", ".", TestOS.All)]
	[Arguments(@"C:\Foo", @"C:\Bar", @"..\Bar", TestOS.Windows)]
	[Arguments(@"C:\Foo", @"C:\Bar\", @"..\Bar\", TestOS.Windows)]
	[Arguments(@"C:\Foo", @"C:\Foo\Bar", "Bar", TestOS.Windows)]
	[Arguments(@"C:\Foo\Bar", @"C:\Foo\", "..", TestOS.Windows)]
	[Arguments(@"C:\Foo", @"C:\Foo\Bar\", @"Bar\", TestOS.Windows)]
	[Arguments(@"C:\Foo\Bar", @"C:\Bar\Bar", @"..\..\Bar\Bar", TestOS.Windows)]
	[Arguments(@"C:\Foo\Foo", @"C:\Foo\Bar", @"..\Bar", TestOS.Windows)]
	[Arguments("/Foo", "/Bar", "../Bar", TestOS.Linux | TestOS.Mac)]
	[Arguments("/", "/Bar", "Bar", TestOS.Linux | TestOS.Mac)]
	[Arguments("/Foo", "/Bar/", "../Bar/", TestOS.Linux | TestOS.Mac)]
	[Arguments("/Foo", "/Foo/Bar", "Bar", TestOS.Linux | TestOS.Mac)]
	[Arguments("/Foo", "/Foo/Bar/", "Bar/", TestOS.Linux | TestOS.Mac)]
	[Arguments("/Foo/Bar", "/Bar/Bar", "../../Bar/Bar", TestOS.Linux | TestOS.Mac)]
	[Arguments("/Foo/Foo", "/Foo/Bar", "../Bar", TestOS.Linux | TestOS.Mac)]
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

	[Test]
	public async Task GetRelativePath_FromAbsolutePathInCurrentDirectory_ShouldReturnRelativePath()
	{
		string rootedPath = FileSystem.Path.Combine(BasePath, "input");
		FileSystem.Directory.CreateDirectory(rootedPath);
		FileSystem.Directory.SetCurrentDirectory(rootedPath);

		string result = FileSystem.Path.GetRelativePath(rootedPath, "a.txt");

		await That(result).IsEqualTo("a.txt");
	}

	[Test]
	[AutoArguments]
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

	[Test]
	public async Task GetRelativePath_RootedPath_ShouldWorkOnAnyDrive()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string rootedPath = "/dir/subDirectory";
		FileSystem.Directory.CreateDirectory(rootedPath);
		string directory = FileSystem.Directory.GetDirectories("/dir").Single();

		string result = FileSystem.Path.GetRelativePath("/dir", directory);

		await That(result).IsEqualTo("subDirectory");
	}

	[Test]
	[AutoArguments]
	public async Task GetRelativePath_ToItself_ShouldReturnDot(string path)
	{
		string expectedResult = ".";

		string result = FileSystem.Path.GetRelativePath(path, path);

		await That(result).IsEqualTo(expectedResult);
	}
}
#endif
