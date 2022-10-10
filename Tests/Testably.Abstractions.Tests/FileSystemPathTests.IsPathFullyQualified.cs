#if FEATURE_PATH_RELATIVE

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.IsPathFullyQualified))]
	public void IsPathFullyQualified_PrefixedRoot_ShouldReturnTrue(
		string directory)
	{
		string path = directory.PrefixRoot();
		bool result = FileSystem.Path.IsPathFullyQualified(path);

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.IsPathFullyQualified))]
	public void IsPathFullyQualified_WithoutPrefixedRoot_ShouldReturnFalse(
		string path)
	{
		bool result = FileSystem.Path.IsPathFullyQualified(path);

		result.Should().BeFalse();
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.IsPathFullyQualified))]
	public void IsPathFullyQualified_Span_PrefixedRoot_ShouldReturnTrue(
		string directory)
	{
		string path = directory.PrefixRoot();
		bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.IsPathFullyQualified))]
	public void IsPathFullyQualified_Span_WithoutPrefixedRoot_ShouldReturnFalse(
		string path)
	{
		bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

		result.Should().BeFalse();
	}
#endif
}
#endif