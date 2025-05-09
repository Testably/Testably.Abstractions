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
	public void IsPathFullyQualified_EdgeCases_ShouldReturnExpectedValue(
		string path, bool expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		bool result = FileSystem.Path
			.IsPathFullyQualified(path);

		result.Should().Be(expected);
	}

	[Theory]
	[AutoData]
	public void IsPathFullyQualified_PrefixedRoot_ShouldReturnTrue(
		string directory)
	{
		string path = FileTestHelper.RootDrive(Test, directory);
		bool result = FileSystem.Path.IsPathFullyQualified(path);

		result.Should().BeTrue();
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void IsPathFullyQualified_Span_PrefixedRoot_ShouldReturnTrue(
		string directory)
	{
		string path = FileTestHelper.RootDrive(Test, directory);
		bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

		result.Should().BeTrue();
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void IsPathFullyQualified_Span_WithoutPrefixedRoot_ShouldReturnFalse(
		string path)
	{
		bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

		result.Should().BeFalse();
	}
#endif

	[Theory]
	[AutoData]
	public void IsPathFullyQualified_WithoutPrefixedRoot_ShouldReturnFalse(
		string path)
	{
		bool result = FileSystem.Path.IsPathFullyQualified(path);

		result.Should().BeFalse();
	}
}
#endif
