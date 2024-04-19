namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetFileNameWithoutExtensionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
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

	[SkippableFact]
	public void GetFileNameWithoutExtension_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileNameWithoutExtension(null);

		result.Should().BeNull();
	}

	[SkippableTheory]
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
	[SkippableTheory]
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

	[SkippableTheory]
	[AutoData]
	public void GetFileNameWithoutExtension_StartingDot_ShouldReturnEmptyString(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + "." + filename;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		result.Should().Be("");
	}

	[SkippableTheory]
	[AutoData]
	public void GetFileNameWithoutExtension_TrailingDot_ShouldReturnFilenameWithoutTrailingDot(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename + ".";

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		result.Should().Be(filename);
	}
}
