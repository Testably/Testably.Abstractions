namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Fact]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.HasExtension))]
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
    [FileSystemTests.Path(nameof(IFileSystem.IPath.HasExtension))]
    public void HasExtension_ShouldReturnExpectedResult(
        string suffix, bool expectedResult, string filename)
    {
        string path = filename + suffix;

        bool result = FileSystem.Path.HasExtension(path);

        result.Should().Be(expectedResult);
    }

#if FEATURE_SPAN
    [Theory]
    [InlineAutoData(".foo", true)]
    [InlineAutoData(".abc.xyz", true)]
    [InlineAutoData("foo", false)]
    [InlineAutoData(".", false)]
    [FileSystemTests.Path(nameof(IFileSystem.IPath.HasExtension))]
    public void HasExtension_Span_ShouldReturnExpectedResult(
        string suffix, bool expectedResult, string filename)
    {
        string path = filename + suffix;

        bool result = FileSystem.Path.HasExtension(path.AsSpan());

        result.Should().Be(expectedResult);
    }
#endif
}