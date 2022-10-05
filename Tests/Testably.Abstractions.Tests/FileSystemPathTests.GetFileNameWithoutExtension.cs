namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableFact]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.GetFileNameWithoutExtension))]
    public void GetFileNameWithoutExtension_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetFileNameWithoutExtension(null);

        result.Should().BeNull();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.GetFileNameWithoutExtension))]
    public void GetFileNameWithoutExtension_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string result = FileSystem.Path.GetFileNameWithoutExtension(path);

        result.Should().Be(filename);
    }

#if FEATURE_SPAN
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.GetFileNameWithoutExtension))]
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