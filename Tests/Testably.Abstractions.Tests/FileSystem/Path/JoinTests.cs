#if FEATURE_PATH_JOIN
namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class JoinTests
{
	[Theory]
	[InlineAutoData((string?)null)]
	[InlineAutoData("")]
	public async Task Join_2Paths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
		string? missingPath, string? path)
	{
		string result1 = FileSystem.Path.Join(path, missingPath);
		string result2 = FileSystem.Path.Join(missingPath, path);

		await That(result1).IsEqualTo(path);
		await That(result2).IsEqualTo(path);
	}

	[Theory]
	[InlineAutoData("/foo/", "/bar/", "/foo//bar/")]
	[InlineAutoData("foo/", "/bar", "foo//bar")]
	[InlineAutoData("foo/", "bar", "foo/bar")]
	[InlineAutoData("foo", "/bar", "foo/bar")]
	[InlineAutoData("foo", "bar", "foo/bar")]
	[InlineAutoData("/foo", "bar/", "/foo/bar/")]
	public async Task Join_2Paths_ShouldReturnExpectedResult(
		string path1, string path2, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Join(path1, path2);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Join_2Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2)
	{
		string expectedResult = path1
								+ FileSystem.Path.DirectorySeparatorChar + path2;

		string result = FileSystem.Path.Join(path1, path2);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Join_2Paths_Span_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2)
	{
		string expectedResult = path1
								+ FileSystem.Path.DirectorySeparatorChar + path2;

		string result = FileSystem.Path.Join(
			path1.AsSpan(),
			path2.AsSpan());

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData((string?)null)]
	[InlineAutoData("")]
	public async Task Join_3Paths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
		string? missingPath, string path1, string path2)
	{
		string expectedPath = $"{path1}{FileSystem.Path.DirectorySeparatorChar}{path2}";

		string result1 = FileSystem.Path.Join(missingPath, path1, path2);
		string result2 = FileSystem.Path.Join(path1, missingPath, path2);
		string result3 = FileSystem.Path.Join(path1, path2, missingPath);

		await That(result1).IsEqualTo(expectedPath);
		await That(result2).IsEqualTo(expectedPath);
		await That(result3).IsEqualTo(expectedPath);
	}

	[Theory]
	[InlineAutoData("/foo/", "/bar/", "/baz/", "/foo//bar//baz/")]
	[InlineAutoData("foo/", "/bar/", "/baz", "foo//bar//baz")]
	[InlineAutoData("foo/", "bar", "/baz", "foo/bar/baz")]
	[InlineAutoData("foo", "/bar", "/baz", "foo/bar/baz")]
	[InlineAutoData("foo", "/bar/", "baz", "foo/bar/baz")]
	[InlineAutoData("foo", "bar", "baz", "foo/bar/baz")]
	[InlineAutoData("/foo", "bar", "baz/", "/foo/bar/baz/")]
	public async Task Join_3Paths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Join(path1, path2, path3);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Join_3Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3)
	{
		string expectedResult = path1
								+ FileSystem.Path.DirectorySeparatorChar + path2
								+ FileSystem.Path.DirectorySeparatorChar + path3;

		string result = FileSystem.Path.Join(path1, path2, path3);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Join_3Paths_Span_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3)
	{
		string expectedResult = path1
								+ FileSystem.Path.DirectorySeparatorChar + path2
								+ FileSystem.Path.DirectorySeparatorChar + path3;

		string result = FileSystem.Path.Join(
			path1.AsSpan(),
			path2.AsSpan(),
			path3.AsSpan());

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData((string?)null)]
	[InlineAutoData("")]
	public async Task Join_4Paths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
		string? missingPath, string path1, string path2, string path3)
	{
		string expectedPath =
			$"{path1}{FileSystem.Path.DirectorySeparatorChar}{path2}{FileSystem.Path.DirectorySeparatorChar}{path3}";

		string result1 = FileSystem.Path.Join(missingPath, path1, path2, path3);
		string result2 = FileSystem.Path.Join(path1, missingPath, path2, path3);
		string result3 = FileSystem.Path.Join(path1, path2, missingPath, path3);
		string result4 = FileSystem.Path.Join(path1, path2, path3, missingPath);

		await That(result1).IsEqualTo(expectedPath);
		await That(result2).IsEqualTo(expectedPath);
		await That(result3).IsEqualTo(expectedPath);
		await That(result4).IsEqualTo(expectedPath);
	}

	[Theory]
	[InlineAutoData("/foo/", "/bar/", "/baz/", "/muh/", "/foo//bar//baz//muh/")]
	[InlineAutoData("foo/", "/bar/", "/baz/", "/muh", "foo//bar//baz//muh")]
	[InlineAutoData("foo/", "bar", "/baz", "/muh", "foo/bar/baz/muh")]
	[InlineAutoData("foo", "/bar", "/baz", "/muh", "foo/bar/baz/muh")]
	[InlineAutoData("foo", "/bar/", "baz/", "muh", "foo/bar/baz/muh")]
	[InlineAutoData("foo", "bar", "baz", "muh", "foo/bar/baz/muh")]
	[InlineAutoData("/foo", "bar", "baz", "muh/", "/foo/bar/baz/muh/")]
	public async Task Join_4Paths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string path4, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path4 = path4.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Join(path1, path2, path3, path4);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Join_4Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4)
	{
		string expectedResult = path1
								+ FileSystem.Path.DirectorySeparatorChar + path2
								+ FileSystem.Path.DirectorySeparatorChar + path3
								+ FileSystem.Path.DirectorySeparatorChar + path4;

		string result = FileSystem.Path.Join(path1, path2, path3, path4);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Join_4Paths_Span_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4)
	{
		string expectedResult = path1
								+ FileSystem.Path.DirectorySeparatorChar + path2
								+ FileSystem.Path.DirectorySeparatorChar + path3
								+ FileSystem.Path.DirectorySeparatorChar + path4;

		string result = FileSystem.Path.Join(
			path1.AsSpan(),
			path2.AsSpan(),
			path3.AsSpan(),
			path4.AsSpan());

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task Join_ParamPaths_Empty_ShouldReturnEmptyString()
	{
		string?[] paths = [];

		string result = FileSystem.Path.Join(paths);

		await That(result).IsEqualTo(string.Empty);
	}

	[Fact]
	public async Task Join_ParamPaths_Null_ShouldThrow()
	{
		void Act()
		{
			_ = FileSystem.Path.Join(null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("paths");
	}

	[Theory]
	[InlineAutoData((string?)null)]
	[InlineAutoData("")]
	public async Task Join_ParamPaths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
		string? missingPath, string path1, string path2, string path3, string path4)
	{
		string expectedPath =
			$"{path1}{FileSystem.Path.DirectorySeparatorChar}{path2}{FileSystem.Path.DirectorySeparatorChar}{path3}{FileSystem.Path.DirectorySeparatorChar}{path4}";

		string result1 =
			FileSystem.Path.Join(new[]
			{
				missingPath,
				path1,
				path2,
				path3,
				path4,
			});
		string result2 =
			FileSystem.Path.Join(new[]
			{
				path1,
				missingPath,
				path2,
				path3,
				path4,
			});
		string result3 =
			FileSystem.Path.Join(new[]
			{
				path1,
				path2,
				missingPath,
				path3,
				path4,
			});
		string result4 =
			FileSystem.Path.Join(new[]
			{
				path1,
				path2,
				path3,
				missingPath,
				path4,
			});
		string result5 =
			FileSystem.Path.Join(new[]
			{
				path1,
				path2,
				path3,
				path4,
				missingPath,
			});

		await That(result1).IsEqualTo(expectedPath);
		await That(result2).IsEqualTo(expectedPath);
		await That(result3).IsEqualTo(expectedPath);
		await That(result4).IsEqualTo(expectedPath);
		await That(result5).IsEqualTo(expectedPath);
	}

	[Theory]
	[InlineAutoData("/foo/", "/bar/", "/baz/", "/muh/", "/maeh/", "/foo//bar//baz//muh//maeh/")]
	[InlineAutoData("foo/", "/bar/", "/baz/", "/muh", "/maeh", "foo//bar//baz//muh/maeh")]
	[InlineAutoData("foo/", "bar", "/baz", "/muh", "/maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("foo", "/bar", "/baz", "/muh", "/maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("foo", "/bar/", "baz/", "muh/", "maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("foo", "bar", "baz", "muh", "maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("/foo", "bar", "baz", "muh", "maeh/", "/foo/bar/baz/muh/maeh/")]
	public async Task Join_ParamPaths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string path4, string path5, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path4 = path4.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path5 = path5.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Join(new[]
		{
			path1,
			path2,
			path3,
			path4,
			path5,
		});

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Join_ParamPaths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4, string path5)
	{
		string expectedResult = path1
								+ FileSystem.Path.DirectorySeparatorChar + path2
								+ FileSystem.Path.DirectorySeparatorChar + path3
								+ FileSystem.Path.DirectorySeparatorChar + path4
								+ FileSystem.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Join(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(expectedResult);
	}

#if FEATURE_PATH_SPAN

	[Fact]
	public async Task Join_ReadOnlySpanPaths_Empty_ShouldReturnEmptyString()
	{
		ReadOnlySpan<string?> paths = Array.Empty<string?>();

		string result = FileSystem.Path.Join(paths);

		await That(result).IsEqualTo(string.Empty);
	}

	[Theory]
	[InlineAutoData((string?)null)]
	[InlineAutoData("")]
	public async Task Join_ReadOnlySpanPaths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
		string? missingPath, string path1, string path2, string path3, string path4)
	{
		string expectedPath =
			$"{path1}{FileSystem.Path.DirectorySeparatorChar}{path2}{FileSystem.Path.DirectorySeparatorChar}{path3}{FileSystem.Path.DirectorySeparatorChar}{path4}";

		string result1 =
			FileSystem.Path.Join(missingPath, path1, path2, path3, path4);
		string result2 =
			FileSystem.Path.Join(path1, missingPath, path2, path3, path4);
		string result3 =
			FileSystem.Path.Join(path1, path2, missingPath, path3, path4);
		string result4 =
			FileSystem.Path.Join(path1, path2, path3, missingPath, path4);
		string result5 =
			FileSystem.Path.Join(path1, path2, path3, path4, missingPath);

		await That(result1).IsEqualTo(expectedPath);
		await That(result2).IsEqualTo(expectedPath);
		await That(result3).IsEqualTo(expectedPath);
		await That(result4).IsEqualTo(expectedPath);
		await That(result5).IsEqualTo(expectedPath);
	}

	[Theory]
	[InlineAutoData("/foo/", "/bar/", "/baz/", "/muh/", "/maeh/", "/foo//bar//baz//muh//maeh/")]
	[InlineAutoData("foo/", "/bar/", "/baz/", "/muh", "/maeh", "foo//bar//baz//muh/maeh")]
	[InlineAutoData("foo/", "bar", "/baz", "/muh", "/maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("foo", "/bar", "/baz", "/muh", "/maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("foo", "/bar/", "baz/", "muh/", "maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("foo", "bar", "baz", "muh", "maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("/foo", "bar", "baz", "muh", "maeh/", "/foo/bar/baz/muh/maeh/")]
	public async Task Join_ReadOnlySpanPaths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string path4, string path5, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path4 = path4.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path5 = path5.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Join(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task Join_ReadOnlySpanPaths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4, string path5)
	{
		string expectedResult = path1
								+ FileSystem.Path.DirectorySeparatorChar + path2
								+ FileSystem.Path.DirectorySeparatorChar + path3
								+ FileSystem.Path.DirectorySeparatorChar + path4
								+ FileSystem.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Join(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(expectedResult);
	}
#endif
}
#endif
