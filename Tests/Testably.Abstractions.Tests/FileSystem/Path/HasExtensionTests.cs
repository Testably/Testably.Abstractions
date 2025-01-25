namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class HasExtensionTests
{
	[Fact]
	public void HasExtension_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Path.HasExtension(null);

		result.Should().BeFalse();
	}

	[Theory]
	[InlineAutoData("abc.", false)]
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
	[InlineAutoData("abc.", false)]
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
