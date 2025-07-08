#if FEATURE_PATH_ADVANCED
namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class EndsInDirectorySeparatorTests
{
	[Fact]
	public async Task EndsInDirectorySeparator_Empty_ShouldReturnExpectedResult()
	{
		bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task EndsInDirectorySeparator_Null_ShouldReturnExpectedResult()
	{
		bool result = FileSystem.Path.EndsInDirectorySeparator(null!);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task EndsInDirectorySeparator_Span_Empty_ShouldReturnExpectedResult()
	{
		bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty.AsSpan());

		await That(result).IsFalse();
	}

	[Theory]
	[InlineAutoData('.')]
	[InlineAutoData('a')]
	public async Task EndsInDirectorySeparator_Span_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
			char lastCharacter, string path)
	{
		path += lastCharacter;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

		await That(result).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task EndsInDirectorySeparator_Span_WithTrailingAltDirectorySeparator_ShouldReturnTrue(
			string path)
	{
		path += FileSystem.Path.AltDirectorySeparatorChar;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task EndsInDirectorySeparator_Span_WithTrailingDirectorySeparator_ShouldReturnTrue(
			string path)
	{
		path += FileSystem.Path.DirectorySeparatorChar;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

		await That(result).IsTrue();
	}

	[Theory]
	[InlineAutoData('.')]
	[InlineAutoData('a')]
	public async Task EndsInDirectorySeparator_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
			char lastCharacter, string path)
	{
		path += lastCharacter;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path);

		await That(result).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task EndsInDirectorySeparator_WithTrailingAltDirectorySeparator_ShouldReturnTrue(
		string path)
	{
		path += FileSystem.Path.AltDirectorySeparatorChar;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path);

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task EndsInDirectorySeparator_WithTrailingDirectorySeparator_ShouldReturnTrue(
		string path)
	{
		path += FileSystem.Path.DirectorySeparatorChar;

		bool result = FileSystem.Path.EndsInDirectorySeparator(path);

		await That(result).IsTrue();
	}
}
#endif
