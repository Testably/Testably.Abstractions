#if FEATURE_PATH_JOIN
namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class TryJoinTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
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
