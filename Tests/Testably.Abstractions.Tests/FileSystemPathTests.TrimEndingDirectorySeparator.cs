#if FEATURE_PATH_ADVANCED
namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.TrimEndingDirectorySeparator))]
    public void TrimEndingDirectorySeparator_DirectoryChar_ShouldTrim(
        string directory)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar;

        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(directory);
    }

    [Fact]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.TrimEndingDirectorySeparator))]
    public void TrimEndingDirectorySeparator_Root_ShouldReturnUnchanged()
    {
        string path = string.Empty.PrefixRoot();

        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(path);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.TrimEndingDirectorySeparator))]
    public void TrimEndingDirectorySeparator_Span_DirectoryChar_ShouldTrim(
        string directory)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar;

        ReadOnlySpan<char> result =
            FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

        result.ToString().Should().Be(directory);
    }

    [Fact]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.TrimEndingDirectorySeparator))]
    public void TrimEndingDirectorySeparator_Span_Root_ShouldReturnUnchanged()
    {
        string path = string.Empty.PrefixRoot();

        ReadOnlySpan<char> result =
            FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

        result.ToString().Should().Be(path);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.TrimEndingDirectorySeparator))]
    public void
        TrimEndingDirectorySeparator_Span_WithoutDirectoryChar_ShouldReturnUnchanged(
            string path)
    {
        ReadOnlySpan<char> result =
            FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

        result.ToString().Should().Be(path);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.TrimEndingDirectorySeparator))]
    public void TrimEndingDirectorySeparator_WithoutDirectoryChar_ShouldReturnUnchanged(
        string path)
    {
        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(path);
    }
}
#endif