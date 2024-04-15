namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetDirectoryNameTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineData((string?)null)]
#if !NETFRAMEWORK
	[InlineData("")]
#endif
	public void GetDirectoryName_NullOrEmpty_ShouldReturnNull(string? path)
	{
		string? result = FileSystem.Path.GetDirectoryName(path);

		result.Should().BeNull();
	}

#if NETFRAMEWORK
	[SkippableTheory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("    ")]
	[InlineData("\t")]
	[InlineData("\n")]
	public void GetDirectoryName_EmptyOrWhiteSpace_ShouldThrowArgumentException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Path.GetDirectoryName(path);
		});

		exception.Should().BeOfType<ArgumentException>();
	}
#endif

#if !NETFRAMEWORK
	[SkippableTheory]
	[InlineData(" ")]
	[InlineData("    ")]
	public void GetDirectoryName_Spaces_ShouldReturnNullOnWindowsOtherwiseEmpty(string? path)
	{
		string? result = FileSystem.Path.GetDirectoryName(path);

		if (Test.RunsOnWindows)
		{
			result.Should().BeNull();
		}
		else
		{
			result.Should().Be("");
		}
	}
#endif

#if !NETFRAMEWORK
	[SkippableTheory]
	[InlineData("\t")]
	[InlineData("\n")]
	public void GetDirectoryName_TabOrNewline_ShouldReturnEmptyString(string? path)
	{
		string? result = FileSystem.Path.GetDirectoryName(path);

		result.Should().Be("");
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void GetDirectoryName_ShouldReturnDirectory(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string? result = FileSystem.Path.GetDirectoryName(path);

		result.Should().Be(directory);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void GetDirectoryName_Span_ShouldReturnDirectory(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetDirectoryName(path.AsSpan());

		result.ToString().Should().Be(directory);
	}
#endif
}
