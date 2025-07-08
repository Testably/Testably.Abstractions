namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class HasExtensionTests
{
	[Fact]
	public async Task HasExtension_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Path.HasExtension(null);

		await That(result).IsFalse();
	}

	[Theory]
	[InlineAutoData("abc.", false)]
	[InlineAutoData(".foo", true)]
	[InlineAutoData(".abc.xyz", true)]
	[InlineAutoData("foo", false)]
	[InlineAutoData(".", false)]
	public async Task HasExtension_ShouldReturnExpectedResult(
		string suffix, bool expectedResult, string filename)
	{
		string path = filename + suffix;

		bool result = FileSystem.Path.HasExtension(path);

		await That(result).IsEqualTo(expectedResult);
	}

#if FEATURE_SPAN
	[Theory]
	[InlineAutoData("abc.", false)]
	[InlineAutoData(".foo", true)]
	[InlineAutoData(".abc.xyz", true)]
	[InlineAutoData("foo", false)]
	[InlineAutoData(".", false)]
	public async Task HasExtension_Span_ShouldReturnExpectedResult(
		string suffix, bool expectedResult, string filename)
	{
		string path = filename + suffix;

		bool result = FileSystem.Path.HasExtension(path.AsSpan());

		await That(result).IsEqualTo(expectedResult);
	}
#endif
}
