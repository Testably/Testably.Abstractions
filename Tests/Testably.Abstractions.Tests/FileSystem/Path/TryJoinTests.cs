#if FEATURE_PATH_JOIN
namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class TryJoinTests
{
	[SkippableTheory]
	[AutoData]
	public void TryJoin_2Paths_BufferTooLittle_ShouldReturnFalse(
		string path1, string path2)
	{
		string expectedResult = path1
		                        + FileSystem.Path.DirectorySeparatorChar + path2;

		char[] buffer = new char[expectedResult.Length - 1];
		Span<char> destination = new(buffer);

		bool result = FileSystem.Path.TryJoin(
			path1.AsSpan(),
			path2.AsSpan(),
			destination,
			out int charsWritten);

		result.Should().BeFalse();
		charsWritten.Should().Be(0);
	}

	[SkippableTheory]
	[InlineAutoData("/foo/", "/bar/", "/foo//bar/")]
	[InlineAutoData("foo/", "/bar", "foo//bar")]
	[InlineAutoData("foo/", "bar", "foo/bar")]
	[InlineAutoData("foo", "/bar", "foo/bar")]
	[InlineAutoData("foo", "bar", "foo/bar")]
	[InlineAutoData("/foo", "bar/", "/foo/bar/")]
	public void TryJoin_2Paths_ShouldReturnExpectedResult(
		string path1, string path2, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		char[] buffer = new char[expectedResult.Length];
		Span<char> destination = new(buffer);

		bool result = FileSystem.Path.TryJoin(
			path1.AsSpan(),
			path2.AsSpan(),
			destination,
			out int charsWritten);

		result.Should().BeTrue();
		charsWritten.Should().Be(expectedResult.Length);
		destination.Slice(0, charsWritten).ToString().Should().Be(expectedResult);
	}

	[SkippableTheory]
	[AutoData]
	public void TryJoin_2Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2)
	{
		string expectedResult = path1
		                        + FileSystem.Path.DirectorySeparatorChar + path2;

		char[] buffer = new char[expectedResult.Length + 10];
		Span<char> destination = new(buffer);

		bool result = FileSystem.Path.TryJoin(
			path1.AsSpan(),
			path2.AsSpan(),
			destination,
			out int charsWritten);

		result.Should().BeTrue();
		charsWritten.Should().Be(expectedResult.Length);
		destination.Slice(0, charsWritten).ToString().Should().Be(expectedResult);
	}

	[SkippableTheory]
	[AutoData]
	public void TryJoin_3Paths_BufferTooLittle_ShouldReturnFalse(
		string path1, string path2, string path3)
	{
		string expectedResult = path1
		                        + FileSystem.Path.DirectorySeparatorChar + path2
		                        + FileSystem.Path.DirectorySeparatorChar + path3;

		char[] buffer = new char[expectedResult.Length - 1];
		Span<char> destination = new(buffer);

		bool result = FileSystem.Path.TryJoin(
			path1.AsSpan(),
			path2.AsSpan(),
			path3.AsSpan(),
			destination,
			out int charsWritten);

		result.Should().BeFalse();
		charsWritten.Should().Be(0);
	}

	[SkippableTheory]
	[InlineAutoData("/foo/", "/bar/", "/baz/", "/foo//bar//baz/")]
	[InlineAutoData("foo/", "/bar/", "/baz", "foo//bar//baz")]
	[InlineAutoData("foo/", "bar", "/baz", "foo/bar/baz")]
	[InlineAutoData("foo", "/bar", "/baz", "foo/bar/baz")]
	[InlineAutoData("foo", "/bar/", "baz", "foo/bar/baz")]
	[InlineAutoData("foo", "bar", "baz", "foo/bar/baz")]
	[InlineAutoData("/foo", "bar", "baz/", "/foo/bar/baz/")]
	public void TryJoin_3Paths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		char[] buffer = new char[expectedResult.Length];
		Span<char> destination = new(buffer);

		bool result = FileSystem.Path.TryJoin(
			path1.AsSpan(),
			path2.AsSpan(),
			path3.AsSpan(),
			destination,
			out int charsWritten);

		result.Should().BeTrue();
		charsWritten.Should().Be(expectedResult.Length);
		destination.Slice(0, charsWritten).ToString().Should().Be(expectedResult);
	}

	[SkippableTheory]
	[AutoData]
	public void TryJoin_3Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3)
	{
		string expectedResult = path1
		                        + FileSystem.Path.DirectorySeparatorChar + path2
		                        + FileSystem.Path.DirectorySeparatorChar + path3;

		char[] buffer = new char[expectedResult.Length + 10];
		Span<char> destination = new(buffer);

		bool result = FileSystem.Path.TryJoin(
			path1.AsSpan(),
			path2.AsSpan(),
			path3.AsSpan(),
			destination,
			out int charsWritten);

		result.Should().BeTrue();
		charsWritten.Should().Be(expectedResult.Length);
		destination.Slice(0, charsWritten).ToString().Should().Be(expectedResult);
	}
}
#endif
