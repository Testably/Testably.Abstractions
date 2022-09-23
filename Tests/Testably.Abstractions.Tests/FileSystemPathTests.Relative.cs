#if FEATURE_PATH_RELATIVE
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
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

    [Theory]
    [AutoData]
    public void GetRelativePath_DifferentDrives_ShouldReturnAbsolutePath(
        string path1, string path2)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Different drives are only supported on Windows
            return;
        }

        path1 = path1.PrefixRoot('A');
        path2 = path2.PrefixRoot('B');
        string result = FileSystem.Path.GetRelativePath(path1, path2);

        result.Should().Be(path2);
    }

    [Theory]
    [AutoData]
    public void GetRelativePath_RootedPath_ShouldReturnAbsolutePath(
        string baseDirectory, string directory1, string directory2)
    {
        baseDirectory = baseDirectory.PrefixRoot();
        string path1 = FileSystem.Path.Combine(baseDirectory, directory1);
        string path2 = FileSystem.Path.Combine(baseDirectory, directory2);
        string expectedRelativePath = FileSystem.Path.Combine("..", directory2);
        string result = FileSystem.Path.GetRelativePath(path1, path2);

        result.Should().Be(expectedRelativePath);
    }

    [Theory]
    [AutoData]
    public void GetRelativePath_ToItself_ShouldReturnDot(string path)
    {
        string expectedResult = ".";

        string result = FileSystem.Path.GetRelativePath(path, path);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void IsPathFullyQualified_PrefixedRoot_ShouldReturnTrue(
        string directory)
    {
        string path = directory.PrefixRoot();
        bool result = FileSystem.Path.IsPathFullyQualified(path);

        result.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void IsPathFullyQualified_WithoutPrefixedRoot_ShouldReturnFalse(
        string path)
    {
        bool result = FileSystem.Path.IsPathFullyQualified(path);

        result.Should().BeFalse();
    }

#if FEATURE_SPAN
    [Theory]
    [AutoData]
    public void IsPathFullyQualified_Span_PrefixedRoot_ShouldReturnTrue(
        string directory)
    {
        string path = directory.PrefixRoot();
        bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

        result.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void IsPathFullyQualified_Span_WithoutPrefixedRoot_ShouldReturnFalse(
        string path)
    {
        bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

        result.Should().BeFalse();
    }
#endif
}
#endif