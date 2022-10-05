#if FEATURE_PATH_ADVANCED
namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableFact]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.EndsInDirectorySeparator))]
    public void EndsInDirectorySeparator_Empty_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty);

        result.Should().BeFalse();
    }

    [SkippableFact]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.EndsInDirectorySeparator))]
    public void EndsInDirectorySeparator_Null_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(null!);

        result.Should().BeFalse();
    }

    [SkippableFact]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.EndsInDirectorySeparator))]
    public void EndsInDirectorySeparator_Span_Empty_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty.AsSpan());

        result.Should().BeFalse();
    }

    [SkippableTheory]
    [InlineAutoData('.')]
    [InlineAutoData('a')]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.EndsInDirectorySeparator))]
    public void
        EndsInDirectorySeparator_Span_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
            char lastCharacter, string path)
    {
        path += lastCharacter;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

        result.Should().BeFalse();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.EndsInDirectorySeparator))]
    public void
        EndsInDirectorySeparator_Span_WithTrailingDirectorySeparator_ShouldReturnTrue(
            string path)
    {
        path += FileSystem.Path.DirectorySeparatorChar;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

        result.Should().BeTrue();
    }

    [SkippableTheory]
    [InlineAutoData('.')]
    [InlineAutoData('a')]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.EndsInDirectorySeparator))]
    public void
        EndsInDirectorySeparator_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
            char lastCharacter, string path)
    {
        path += lastCharacter;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path);

        result.Should().BeFalse();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.EndsInDirectorySeparator))]
    public void EndsInDirectorySeparator_WithTrailingDirectorySeparator_ShouldReturnTrue(
        string path)
    {
        path += FileSystem.Path.DirectorySeparatorChar;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path);

        result.Should().BeTrue();
    }
}
#endif