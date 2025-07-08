namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetExtensionTests
{
	[Fact]
	public async Task GetExtension_Empty_ShouldReturnEmpty()
	{
		string? result = FileSystem.Path.GetExtension(string.Empty);

		await That(result).IsEmpty();
	}

	[Fact]
	public async Task GetExtension_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetExtension(null);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task GetExtension_ShouldReturnExtensionWithLeadingDot(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		string result = FileSystem.Path.GetExtension(path);

		await That(result).IsEqualTo("." + extension);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task GetExtension_Span_ShouldReturnExtensionWithLeadingDot(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetExtension(path.AsSpan());

		await That(result.ToString()).IsEqualTo("." + extension);
	}
#endif

	[Theory]
	[AutoData]
	public async Task GetExtension_StartingDot_ShouldReturnCompleteFileName(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + "." + filename;

		string result = FileSystem.Path.GetExtension(path);

		await That(result).IsEqualTo("." + filename);
	}

	[Theory]
	[AutoData]
	public async Task GetExtension_TrailingDot_ShouldReturnEmptyString(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename + ".";

		string result = FileSystem.Path.GetExtension(path);

		await That(result).IsEqualTo("");
	}
}
