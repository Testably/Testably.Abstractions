#if FEATURE_PATH_ADVANCED
namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class TrimEndingDirectorySeparatorTests
{
	[Theory]
	[AutoData]
	public void TrimEndingDirectorySeparator_DirectoryChar_ShouldTrim(
		string directory)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar;

		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		result.Should().Be(directory);
	}

	[Fact]
	public void TrimEndingDirectorySeparator_EmptyString_ShouldReturnEmptyString()
	{
		string result = FileSystem.Path.TrimEndingDirectorySeparator(string.Empty);

		result.Should().Be(string.Empty);
	}

	[Fact]
	public void TrimEndingDirectorySeparator_Root_ShouldReturnUnchanged()
	{
		string path = FileTestHelper.RootDrive(Test);

		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		result.Should().Be(path);
	}

	[Theory]
	[AutoData]
	public void TrimEndingDirectorySeparator_Span_DirectoryChar_ShouldTrim(
		string directory)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar;

		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		result.ToString().Should().Be(directory);
	}

	[Fact]
	public void TrimEndingDirectorySeparator_Span_Root_ShouldReturnUnchanged()
	{
		string path = FileTestHelper.RootDrive(Test);

		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		result.ToString().Should().Be(path);
	}

	[Theory]
	[AutoData]
	public void
		TrimEndingDirectorySeparator_Span_WithoutDirectoryChar_ShouldReturnUnchanged(
			string path)
	{
		ReadOnlySpan<char> result =
			FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

		result.ToString().Should().Be(path);
	}

	[Theory]
	[AutoData]
	public void TrimEndingDirectorySeparator_WithoutDirectoryChar_ShouldReturnUnchanged(
		string path)
	{
		string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

		result.Should().Be(path);
	}
}
#endif
