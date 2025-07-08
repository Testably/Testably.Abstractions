#if FEATURE_PATH_RELATIVE
namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class IsPathFullyQualifiedTests
{
	[Theory]
	[InlineData("C", false, TestOS.Windows)]
	[InlineData("//", true, TestOS.All)]
	[InlineData("/Foo", true, TestOS.Linux | TestOS.Mac)]
	[InlineData("/Foo", false, TestOS.Windows)]
	[InlineData(@"\\", true, TestOS.Windows)]
	[InlineData("/?", true, TestOS.Windows)]
	[InlineData(@"\?", true, TestOS.Windows)]
	public async Task IsPathFullyQualified_EdgeCases_ShouldReturnExpectedValue(
		string path, bool expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		bool result = FileSystem.Path
			.IsPathFullyQualified(path);

		await That(result).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public async Task IsPathFullyQualified_PrefixedRoot_ShouldReturnTrue(
		string directory)
	{
		string path = FileTestHelper.RootDrive(Test, directory);
		bool result = FileSystem.Path.IsPathFullyQualified(path);

		await That(result).IsTrue();
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task IsPathFullyQualified_Span_PrefixedRoot_ShouldReturnTrue(
		string directory)
	{
		string path = FileTestHelper.RootDrive(Test, directory);
		bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

		await That(result).IsTrue();
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task IsPathFullyQualified_Span_WithoutPrefixedRoot_ShouldReturnFalse(
		string path)
	{
		bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

		await That(result).IsFalse();
	}
#endif

	[Theory]
	[AutoData]
	public async Task IsPathFullyQualified_WithoutPrefixedRoot_ShouldReturnFalse(
		string path)
	{
		bool result = FileSystem.Path.IsPathFullyQualified(path);

		await That(result).IsFalse();
	}
}
#endif
