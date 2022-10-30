#if FEATURE_PATH_RELATIVE
namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class IsPathFullyQualifiedTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void IsPathFullyQualified_PrefixedRoot_ShouldReturnTrue(
		string directory)
	{
		string path = FileTestHelper.RootDrive(directory);
		bool result = FileSystem.Path.IsPathFullyQualified(path);

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void IsPathFullyQualified_WithoutPrefixedRoot_ShouldReturnFalse(
		string path)
	{
		bool result = FileSystem.Path.IsPathFullyQualified(path);

		result.Should().BeFalse();
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void IsPathFullyQualified_Span_PrefixedRoot_ShouldReturnTrue(
		string directory)
	{
		string path = FileTestHelper.RootDrive(directory);
		bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void IsPathFullyQualified_Span_WithoutPrefixedRoot_ShouldReturnFalse(
		string path)
	{
		bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

		result.Should().BeFalse();
	}
#endif
}
#endif