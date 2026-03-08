namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class HasExtensionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task HasExtension_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Path.HasExtension(null);

		await That(result).IsFalse();
	}

	[Test]
	[AutoArguments("abc.", false)]
	[AutoArguments(".foo", true)]
	[AutoArguments(".abc.xyz", true)]
	[AutoArguments("foo", false)]
	[AutoArguments(".", false)]
	public async Task HasExtension_ShouldReturnExpectedResult(
		string suffix, bool expectedResult, string filename)
	{
		string path = filename + suffix;

		bool result = FileSystem.Path.HasExtension(path);

		await That(result).IsEqualTo(expectedResult);
	}

#if FEATURE_SPAN
	[Test]
	[AutoArguments("abc.", false)]
	[AutoArguments(".foo", true)]
	[AutoArguments(".abc.xyz", true)]
	[AutoArguments("foo", false)]
	[AutoArguments(".", false)]
	public async Task HasExtension_Span_ShouldReturnExpectedResult(
		string suffix, bool expectedResult, string filename)
	{
		string path = filename + suffix;

		bool result = FileSystem.Path.HasExtension(path.AsSpan());

		await That(result).IsEqualTo(expectedResult);
	}
#endif
}
