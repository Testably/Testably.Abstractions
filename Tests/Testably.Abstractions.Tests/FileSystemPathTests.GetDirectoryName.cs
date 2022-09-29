namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Fact]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.GetDirectoryName))]
    public void GetDirectoryName_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetDirectoryName(null);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.GetDirectoryName))]
    public void GetDirectoryName_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string? result = FileSystem.Path.GetDirectoryName(path);

        result.Should().Be(directory);
    }
#if FEATURE_SPAN
    [Theory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.GetDirectoryName))]
    public void GetDirectoryName_Span_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        ReadOnlySpan<char> result = FileSystem.Path.GetDirectoryName(path.AsSpan());

        result.ToString().Should().Be(directory);
    }
#endif
}