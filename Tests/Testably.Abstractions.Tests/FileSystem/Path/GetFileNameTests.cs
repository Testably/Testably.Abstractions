namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetFileNameTests
{
	[Fact]
	public async Task GetFileName_EmptyString_ShouldReturnEmptyString()
	{
		string result = FileSystem.Path.GetFileName(string.Empty);

		await That(result).IsEqualTo(string.Empty);
	}

	[Fact]
	public async Task GetFileName_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileName(null);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task GetFileName_ShouldReturnFilename(string directory, string filename,
		string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		string result = FileSystem.Path.GetFileName(path);

		await That(result).IsEqualTo(filename + "." + extension);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task GetFileName_Span_ShouldReturnDirectory(
		string directory, string filename,
		string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetFileName(path.AsSpan());

		await That(result.ToString()).IsEqualTo(filename + "." + extension);
	}
#endif

	[Theory]
	[InlineData("foo/", "", TestOS.All)]
	[InlineData("bar\\", "", TestOS.Windows)]
	[InlineData("/foo", "foo", TestOS.All)]
	[InlineData("\\bar", "bar", TestOS.Windows)]
	public async Task GetFileName_SpecialCases_ShouldReturnExpectedResult(
		string? path, string? expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string? result = FileSystem.Path.GetFileName(path);

		await That(result).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public async Task GetFileName_WithoutDirectory_ShouldReturnFilename(string filename)
	{
		string result = FileSystem.Path.GetFileName(filename);

		await That(result).IsEqualTo(filename);
	}
}
