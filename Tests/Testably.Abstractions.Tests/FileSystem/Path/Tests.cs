namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void AltDirectorySeparatorChar_ShouldReturnDefaultValue()
	{
		char result = FileSystem.Path.AltDirectorySeparatorChar;

		result.Should().Be(System.IO.Path.AltDirectorySeparatorChar);
	}

	[SkippableFact]
	public void DirectorySeparatorChar_ShouldReturnDefaultValue()
	{
		char result = FileSystem.Path.DirectorySeparatorChar;

		result.Should().Be(System.IO.Path.DirectorySeparatorChar);
	}

	[SkippableFact]
	public void GetInvalidFileNameChars_ShouldReturnDefaultValue()
	{
		char[] result = FileSystem.Path.GetInvalidFileNameChars();

		result.Should().BeEquivalentTo(System.IO.Path.GetInvalidFileNameChars());
	}

	[SkippableTheory]
	[InlineData("D:")]
	public void GetPathRoot_WithoutTrailingSeparatorChar_ShouldReturnDefaultValue(string path)
	{
		var expectedResult = System.IO.Path.GetPathRoot(path);
		string? result = FileSystem.Path.GetPathRoot(path);

		result.Should().Be(expectedResult);
	}

	[SkippableTheory]
	[AutoData]
	public void GetPathRoot_ShouldReturnDefaultValue(string path)
	{
		string? result = FileSystem.Path.GetPathRoot(path);

		result.Should().Be(System.IO.Path.GetPathRoot(path));
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void GetPathRoot_Span_ShouldReturnDefaultValue(string path)
	{
		ReadOnlySpan<char> result = FileSystem.Path.GetPathRoot(path.AsSpan());

		result.ToArray().Should().BeEquivalentTo(
			System.IO.Path.GetPathRoot(path.AsSpan()).ToArray());
	}
#endif

	[SkippableTheory]
	[InlineData("/foo")]
	[InlineData("foo/bar")]
	public void IsPathRooted_ShouldReturnDefaultValue(string path)
	{
		bool result = FileSystem.Path.IsPathRooted(path);

		result.Should().Be(System.IO.Path.IsPathRooted(path));
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[InlineData("/foo")]
	[InlineData("foo/bar")]
	public void IsPathRooted_Span_ShouldReturnDefaultValue(string path)
	{
		bool result = FileSystem.Path.IsPathRooted(path.AsSpan());

		result.Should().Be(System.IO.Path.IsPathRooted(path.AsSpan()));
	}
#endif

	[SkippableFact]
	public void PathSeparator_ShouldReturnDefaultValue()
	{
		char result = FileSystem.Path.PathSeparator;

		result.Should().Be(System.IO.Path.PathSeparator);
	}

	[SkippableFact]
	public void VolumeSeparatorChar_ShouldReturnDefaultValue()
	{
		char result = FileSystem.Path.VolumeSeparatorChar;

		result.Should().Be(System.IO.Path.VolumeSeparatorChar);
	}
}
