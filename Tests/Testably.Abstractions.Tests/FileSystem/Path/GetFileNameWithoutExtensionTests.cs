namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetFileNameWithoutExtensionTests
{
	[Theory]
	[AutoData]
	public void GetFileNameWithoutExtension_MultipleDots_ShouldReturnOnlyRemoveTheLastExtension(
		string directory, string filename1, string filename2, string extension)
	{
		string filename = filename1 + "." + filename2;
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		result.Should().Be(filename);
	}

	[Fact]
	public void GetFileNameWithoutExtension_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileNameWithoutExtension(null);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void GetFileNameWithoutExtension_ShouldReturnFileNameWithoutExtension(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		result.Should().Be(filename);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void GetFileNameWithoutExtension_Span_ShouldReturnFileNameWithoutExtension(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result =
			FileSystem.Path.GetFileNameWithoutExtension(path.AsSpan());

		result.ToString().Should().Be(filename);
	}
#endif

	[Theory]
	[AutoData]
	public void GetFileNameWithoutExtension_StartingDot_ShouldReturnEmptyString(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + "." + filename;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		result.Should().Be("");
	}

	[Theory]
	[AutoData]
	public void GetFileNameWithoutExtension_TrailingDot_ShouldReturnFilenameWithoutTrailingDot(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename + ".";

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		result.Should().Be(filename);
	}
}
