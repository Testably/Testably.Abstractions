#if FEATURE_PATH_ADVANCED
namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class EndsInDirectorySeparatorTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void EndsInDirectorySeparator_Empty_ShouldReturnExpectedResult()
	{
		bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty);

		result.Should().BeFalse();
	}

	[SkippableFact]
	public void EndsInDirectorySeparator_Null_ShouldReturnExpectedResult()
	{
		bool result = FileSystem.Path.EndsInDirectorySeparator(null!);

		result.Should().BeFalse();
	}

	[SkippableFact]
	public void EndsInDirectorySeparator_Span_Empty_ShouldReturnExpectedResult()
	{
		bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty.AsSpan());

		result.Should().BeFalse();
	}

	[SkippableTheory]
	[InlineAutoData('.')]
	[InlineAutoData('a')]
	public void
		EndsInDirectorySeparator_Span_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
			char lastCharacter, string path)
	{
		path += lastCharacter;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

		result.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void
		EndsInDirectorySeparator_Span_WithTrailingDirectorySeparator_ShouldReturnTrue(
			string path)
	{
		path += FileSystem.Path.DirectorySeparatorChar;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[InlineAutoData('.')]
	[InlineAutoData('a')]
	public void
		EndsInDirectorySeparator_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
			char lastCharacter, string path)
	{
		path += lastCharacter;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path);

		result.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void EndsInDirectorySeparator_WithTrailingDirectorySeparator_ShouldReturnTrue(
		string path)
	{
		path += FileSystem.Path.DirectorySeparatorChar;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path);

		result.Should().BeTrue();
	}
}
#endif
