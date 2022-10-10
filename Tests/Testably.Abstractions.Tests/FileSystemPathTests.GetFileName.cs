namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.GetFileName))]
	public void GetFileName_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileName(null);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.GetFileName))]
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
	[FileSystemTests.Path(nameof(IFileSystem.IPath.GetFileName))]
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