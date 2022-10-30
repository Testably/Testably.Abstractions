namespace Testably.Abstractions.Tests.FileSystem.Path;

public abstract partial class PathTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetFileName_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileName(null);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void GetFileName_ShouldReturnDirectory(string directory, string filename,
	                                              string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetFileName(path);

		result.Should().Be(filename + "." + extension);
	}

#if FEATURE_SPAN
	[SkippableTheory]
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
}