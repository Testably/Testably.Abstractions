namespace Testably.Abstractions.Tests.FileSystem.Path;

public abstract partial class FileSystemPathTests<TFileSystem>
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
	public void GetExtension_ShouldReturnDirectory(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetExtension(path);

		result.Should().Be("." + extension);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	
	public void GetExtension_Span_ShouldReturnDirectory(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetExtension(path.AsSpan());

		result.ToString().Should().Be("." + extension);
	}
#endif
}