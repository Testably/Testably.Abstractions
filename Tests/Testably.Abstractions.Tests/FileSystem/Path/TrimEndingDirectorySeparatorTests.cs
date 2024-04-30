#if FEATURE_PATH_ADVANCED
namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class TrimEndingDirectorySeparatorTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void TrimEndingDirectorySeparator_DirectoryChar_ShouldTrim(
		string directory)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar;

		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		result.Should().Be(directory);
	}

	[SkippableFact]
	public void TrimEndingDirectorySeparator_EmptyString_ShouldReturnEmptyString()
	{
		string result = FileSystem.Path.TrimEndingDirectorySeparator(string.Empty);

		result.Should().Be(string.Empty);
	}

	[SkippableFact]
	public void TrimEndingDirectorySeparator_Root_ShouldReturnUnchanged()
	{
		string path = FileTestHelper.RootDrive(Test);

		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		result.Should().Be(path);
	}

	[SkippableTheory]
	[AutoData]
	public void TrimEndingDirectorySeparator_Span_DirectoryChar_ShouldTrim(
		string directory)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar;

		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		result.ToString().Should().Be(directory);
	}

	[SkippableFact]
	public void TrimEndingDirectorySeparator_Span_Root_ShouldReturnUnchanged()
	{
		string path = FileTestHelper.RootDrive(Test);

		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		result.ToString().Should().Be(path);
	}

	[SkippableTheory]
	[AutoData]
	public void
		TrimEndingDirectorySeparator_Span_WithoutDirectoryChar_ShouldReturnUnchanged(
			string path)
	{
		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		result.ToString().Should().Be(path);
	}

	[SkippableTheory]
	[AutoData]
	public void TrimEndingDirectorySeparator_WithoutDirectoryChar_ShouldReturnUnchanged(
		string path)
	{
		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		result.Should().Be(path);
	}
}
#endif
