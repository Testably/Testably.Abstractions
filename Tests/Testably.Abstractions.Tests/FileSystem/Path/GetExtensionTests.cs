namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetExtensionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetExtension_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetExtension(null);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void GetExtension_ShouldReturnExtension(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetExtension(path);

		result.Should().Be("." + extension);
	}

	[SkippableTheory]
	[AutoData]
	public void GetExtension_TrailingDot_ShouldReturnEmptyString(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename + ".";

		string result = FileSystem.Path.GetExtension(path);

		result.Should().Be("");
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void GetExtension_Span_ShouldReturnExtension(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetExtension(path.AsSpan());

		result.ToString().Should().Be("." + extension);
	}
#endif
}