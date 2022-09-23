#if FEATURE_PATH_ADVANCED
namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Fact]
    public void EndsInDirectorySeparator_Empty_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty);

        result.Should().BeFalse();
    }

    [Fact]
    public void EndsInDirectorySeparator_Null_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(null!);

        result.Should().BeFalse();
    }

    [Fact]
    public void EndsInDirectorySeparator_Span_Empty_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty.AsSpan());

        result.Should().BeFalse();
    }

    [Theory]
    [InlineAutoData('.')]
    [InlineAutoData('a')]
    public void
        EndsInDirectorySeparator_Span_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
            char lastCharacter, string path)
    {
        path += lastCharacter;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

        result.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void
        EndsInDirectorySeparator_Span_WithTrailingDirectorySeparator_ShouldReturnTrue(
            string path)
    {
        path += FileSystem.Path.DirectorySeparatorChar;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

        result.Should().BeTrue();
    }

    [Theory]
    [InlineAutoData('.')]
    [InlineAutoData('a')]
    public void
        EndsInDirectorySeparator_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
            char lastCharacter, string path)
    {
        path += lastCharacter;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path);

        result.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void EndsInDirectorySeparator_WithTrailingDirectorySeparator_ShouldReturnTrue(
        string path)
    {
        path += FileSystem.Path.DirectorySeparatorChar;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path);

        result.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void TrimEndingDirectorySeparator_DirectoryChar_ShouldTrim(
        string directory)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar;

        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(directory);
    }

    [Fact]
    public void TrimEndingDirectorySeparator_Root_ShouldReturnUnchanged()
    {
        string path = string.Empty.PrefixRoot();

        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(path);
    }

    [Theory]
    [AutoData]
    public void TrimEndingDirectorySeparator_Span_DirectoryChar_ShouldTrim(
        string directory)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar;

        ReadOnlySpan<char> result =
            FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

        result.ToString().Should().Be(directory);
    }

    [Fact]
    public void TrimEndingDirectorySeparator_Span_Root_ShouldReturnUnchanged()
    {
        string path = string.Empty.PrefixRoot();

        ReadOnlySpan<char> result =
            FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

        result.ToString().Should().Be(path);
    }

    [Theory]
    [AutoData]
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
    public void TrimEndingDirectorySeparator_WithoutDirectoryChar_ShouldReturnUnchanged(
        string path)
    {
        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(path);
    }
}
#endif