namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Fact]
    public void GetFileName_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetFileName(null);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void GetFileName_ShouldReturnDirectory(string directory, string filename,
                                                  string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string result = FileSystem.Path.GetFileName(path);

        result.Should().Be(filename + "." + extension);
    }

    [Fact]
    public void GetFileNameWithoutExtension_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetFileNameWithoutExtension(null);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void GetFileNameWithoutExtension_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string result = FileSystem.Path.GetFileNameWithoutExtension(path);

        result.Should().Be(filename);
    }
#if FEATURE_SPAN
    [Theory]
    [AutoData]
    public void GetFileName_Span_ShouldReturnDirectory(
        string directory, string filename,
        string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        ReadOnlySpan<char> result = FileSystem.Path.GetFileName(path.AsSpan());

        result.ToString().Should().Be(filename + "." + extension);
    }

    [Theory]
    [AutoData]
    public void GetFileNameWithoutExtension_Span_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        ReadOnlySpan<char> result =
            FileSystem.Path.GetFileNameWithoutExtension(path.AsSpan());

        result.ToString().Should().Be(filename);
    }
#endif
}