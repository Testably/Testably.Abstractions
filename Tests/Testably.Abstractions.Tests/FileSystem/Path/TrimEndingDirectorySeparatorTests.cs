#if FEATURE_PATH_ADVANCED
namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class TrimEndingDirectorySeparatorTests
{
	[Theory]
	[AutoData]
	public async Task TrimEndingDirectorySeparator_DirectoryChar_ShouldTrim(
		string directory)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar;

		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		await That(result).IsEqualTo(directory);
	}

	[Fact]
	public async Task TrimEndingDirectorySeparator_EmptyString_ShouldReturnEmptyString()
	{
		string result = FileSystem.Path.TrimEndingDirectorySeparator(string.Empty);

		await That(result).IsEqualTo(string.Empty);
	}

	[Fact]
	public async Task TrimEndingDirectorySeparator_Root_ShouldReturnUnchanged()
	{
		string path = FileTestHelper.RootDrive(Test);

		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		await That(result).IsEqualTo(path);
	}

	[Theory]
	[AutoData]
	public async Task TrimEndingDirectorySeparator_Span_DirectoryChar_ShouldTrim(
		string directory)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar;

		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		await That(result.ToString()).IsEqualTo(directory);
	}

	[Fact]
	public async Task TrimEndingDirectorySeparator_Span_Root_ShouldReturnUnchanged()
	{
		string path = FileTestHelper.RootDrive(Test);

		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		await That(result.ToString()).IsEqualTo(path);
	}

	[Theory]
	[AutoData]
	public async Task
		TrimEndingDirectorySeparator_Span_WithoutDirectoryChar_ShouldReturnUnchanged(
			string path)
	{
		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		await That(result.ToString()).IsEqualTo(path);
	}

	[Theory]
	[AutoData]
	public async Task TrimEndingDirectorySeparator_WithoutDirectoryChar_ShouldReturnUnchanged(
		string path)
	{
		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		await That(result).IsEqualTo(path);
	}
}
#endif
