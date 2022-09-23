namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    public void ChangeExtension_EmptyPath_ShouldReturnEmptyString(string extension)
    {
        string result = FileSystem.Path.ChangeExtension(string.Empty, extension);

        result.Should().BeEmpty();
    }

    [Theory]
    [AutoData]
    public void ChangeExtension_NullPath_ShouldReturnNull(string extension)
    {
        string? result = FileSystem.Path.ChangeExtension(null, extension);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void ChangeExtension_WithDirectory_ShouldIncludeDirectory(
        string directory, string fileName, string extension)
    {
        string path = FileSystem.Path.Combine(directory, fileName + ".foo");
        string expectedResult =
            FileSystem.Path.Combine(directory, fileName + "." + extension);

        string result = FileSystem.Path.ChangeExtension(path, extension);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void ChangeExtension_WithLeadingDotInExtension_ShouldNotIncludeTwoDots(
        string fileName, string extension)
    {
        string path = fileName + ".foo";
        string expectedResult = fileName + "." + extension;

        string result = FileSystem.Path.ChangeExtension(path, "." + extension);

        result.Should().Be(expectedResult);
    }

    [Fact]
    public void GetExtension_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetExtension(null);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void GetExtension_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string result = FileSystem.Path.GetExtension(path);

        result.Should().Be("." + extension);
    }

    [Fact]
    public void HasExtension_Null_ShouldReturnFalse()
    {
        bool result = FileSystem.Path.HasExtension(null);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineAutoData(".foo", true)]
    [InlineAutoData(".abc.xyz", true)]
    [InlineAutoData("foo", false)]
    [InlineAutoData(".", false)]
    public void HasExtension_ShouldReturnExpectedResult(
        string suffix, bool expectedResult, string filename)
    {
        string path = filename + suffix;

        bool result = FileSystem.Path.HasExtension(path);

        result.Should().Be(expectedResult);
    }
#if FEATURE_SPAN
    [Theory]
    [AutoData]
    public void GetExtension_Span_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        ReadOnlySpan<char> result = FileSystem.Path.GetExtension(path.AsSpan());

        result.ToString().Should().Be("." + extension);
    }

    [Theory]
    [InlineAutoData(".foo", true)]
    [InlineAutoData(".abc.xyz", true)]
    [InlineAutoData("foo", false)]
    [InlineAutoData(".", false)]
    public void HasExtension_Span_ShouldReturnExpectedResult(
        string suffix, bool expectedResult, string filename)
    {
        string path = filename + suffix;

        bool result = FileSystem.Path.HasExtension(path.AsSpan());

        result.Should().Be(expectedResult);
    }
#endif
}