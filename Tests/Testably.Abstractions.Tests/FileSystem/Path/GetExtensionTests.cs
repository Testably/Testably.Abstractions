namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetExtensionTests
{
	[Fact]
	public void GetExtension_Empty_ShouldReturnEmpty()
	{
		string? result = FileSystem.Path.GetExtension(string.Empty);

		result.Should().BeEmpty();
	}

	[Fact]
	public void GetExtension_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetExtension(null);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void GetExtension_ShouldReturnExtensionWithLeadingDot(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetExtension(path);

		result.Should().Be("." + extension);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void GetExtension_Span_ShouldReturnExtensionWithLeadingDot(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetExtension(path.AsSpan());

		result.ToString().Should().Be("." + extension);
	}
#endif

	[Theory]
	[AutoData]
	public void GetExtension_StartingDot_ShouldReturnCompleteFileName(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + "." + filename;

		string result = FileSystem.Path.GetExtension(path);

		result.Should().Be("." + filename);
	}

	[Theory]
	[AutoData]
	public void GetExtension_TrailingDot_ShouldReturnEmptyString(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename + ".";

		string result = FileSystem.Path.GetExtension(path);

		result.Should().Be("");
	}
}
