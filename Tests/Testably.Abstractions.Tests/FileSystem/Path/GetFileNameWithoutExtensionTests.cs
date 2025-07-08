namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetFileNameWithoutExtensionTests
{
	[Theory]
	[AutoData]
	public async Task GetFileNameWithoutExtension_MultipleDots_ShouldReturnOnlyRemoveTheLastExtension(
		string directory, string filename1, string filename2, string extension)
	{
		string filename = filename1 + "." + filename2;
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		await That(result).IsEqualTo(filename);
	}

	[Fact]
	public async Task GetFileNameWithoutExtension_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileNameWithoutExtension(null);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task GetFileNameWithoutExtension_ShouldReturnFileNameWithoutExtension(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		await That(result).IsEqualTo(filename);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task GetFileNameWithoutExtension_Span_ShouldReturnFileNameWithoutExtension(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		ReadOnlySpan<char> result =
			FileSystem.Path.GetFileNameWithoutExtension(path.AsSpan());

		await That(result.ToString()).IsEqualTo(filename);
	}
#endif

	[Theory]
	[AutoData]
	public async Task GetFileNameWithoutExtension_StartingDot_ShouldReturnEmptyString(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + "." + filename;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		await That(result).IsEqualTo("");
	}

	[Theory]
	[AutoData]
	public async Task GetFileNameWithoutExtension_TrailingDot_ShouldReturnFilenameWithoutTrailingDot(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename + ".";

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		await That(result).IsEqualTo(filename);
	}
}
