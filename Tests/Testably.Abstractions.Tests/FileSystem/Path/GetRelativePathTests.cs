#if FEATURE_PATH_RELATIVE
namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetRelativePathTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void GetRelativePath_CommonParentDirectory_ShouldReturnRelativePath(
		string baseDirectory, string directory1, string directory2)
	{
		string path1 = FileSystem.Path.Combine(baseDirectory, directory1);
		string path2 = FileSystem.Path.Combine(baseDirectory, directory2);
		string expectedRelativePath = FileSystem.Path.Combine("..", directory2);
		string result = FileSystem.Path.GetRelativePath(path1, path2);

		result.Should().Be(expectedRelativePath);
	}

	[SkippableTheory]
	[AutoData]
	public void GetRelativePath_DifferentDrives_ShouldReturnAbsolutePath(
		string path1, string path2)
	{
		if (!Test.RunsOnWindows)
		{
			// Different drives are only supported on Windows
			return;
		}

		path1 = FileTestHelper.RootDrive(path1, 'A');
		path2 = FileTestHelper.RootDrive(path2, 'B');
		string result = FileSystem.Path.GetRelativePath(path1, path2);

		result.Should().Be(path2);
	}

	[SkippableTheory]
	[AutoData]
	public void GetRelativePath_RootedPath_ShouldReturnAbsolutePath(
		string baseDirectory, string directory1, string directory2)
	{
		baseDirectory = FileTestHelper.RootDrive(baseDirectory);
		string path1 = FileSystem.Path.Combine(baseDirectory, directory1);
		string path2 = FileSystem.Path.Combine(baseDirectory, directory2);
		string expectedRelativePath = FileSystem.Path.Combine("..", directory2);
		string result = FileSystem.Path.GetRelativePath(path1, path2);

		result.Should().Be(expectedRelativePath);
	}

	[SkippableTheory]
	[AutoData]
	public void GetRelativePath_ToItself_ShouldReturnDot(string path)
	{
		string expectedResult = ".";

		string result = FileSystem.Path.GetRelativePath(path, path);

		result.Should().Be(expectedResult);
	}
}
#endif
