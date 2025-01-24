namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetFileNameTests
{
	[Fact]
	public void GetFileName_EmptyString_ShouldReturnEmptyString()
	{
		string result = FileSystem.Path.GetFileName(string.Empty);

		result.Should().Be(string.Empty);
	}

	[Fact]
	public void GetFileName_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileName(null);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void GetFileName_ShouldReturnFilename(string directory, string filename,
		string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetFileName(path);

		result.Should().Be(filename + "." + extension);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void GetFileName_Span_ShouldReturnDirectory(
		string directory, string filename,
		string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetFileName(path.AsSpan());

		result.ToString().Should().Be(filename + "." + extension);
	}
#endif

	[Theory]
	[InlineData("foo/", "", TestOS.All)]
	[InlineData("bar\\", "", TestOS.Windows)]
	[InlineData("/foo", "foo", TestOS.All)]
	[InlineData("\\bar", "bar", TestOS.Windows)]
	public void GetFileName_SpecialCases_ShouldReturnExpectedResult(
		string? path, string? expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string? result = FileSystem.Path.GetFileName(path);

		result.Should().Be(expected);
	}

	[Theory]
	[AutoData]
	public void GetFileName_WithoutDirectory_ShouldReturnFilename(string filename)
	{
		string result = FileSystem.Path.GetFileName(filename);

		result.Should().Be(filename);
	}
}
