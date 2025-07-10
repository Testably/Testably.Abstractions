namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class CombineTests
{
	[Theory]
	[AutoData]
	public async Task Combine_2Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string path)
	{
		string result1 = FileSystem.Path.Combine(path, string.Empty);
		string result2 = FileSystem.Path.Combine(string.Empty, path);

		await That(result1).IsEqualTo(path);
		await That(result2).IsEqualTo(path);
	}

	[Theory]
	[AutoData]
	public async Task Combine_2Paths_OneNull_ShouldThrowArgumentNullException(string pathA)
	{
		void Act1() =>
			FileSystem.Path.Combine(pathA, null!);

		void Act2() =>
			FileSystem.Path.Combine(null!, pathA);

		await That(Act1).Throws<ArgumentNullException>().WithParamName("path2").And
			.WithHResult(-2147467261);
		await That(Act2).Throws<ArgumentNullException>().WithParamName("path1").And
			.WithHResult(-2147467261);
	}

	[Theory]
	[AutoData]
	public async Task Combine_2Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;

		string result = FileSystem.Path.Combine(path1, path2);

		await That(result).IsEqualTo(path2);
	}

	[Theory]
	[InlineData("", "", "")]
	[InlineData("/foo/", "/bar/", "/bar/")]
	[InlineData("foo/", "/bar", "/bar")]
	[InlineData("foo/", "bar", "foo/bar")]
	[InlineData("foo", "/bar", "/bar")]
	[InlineData("foo", "bar", "foo/bar")]
	[InlineData("/foo", "bar/", "/foo/bar/")]
	public async Task Combine_2Paths_ShouldReturnExpectedResult(
		string path1, string path2, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	public async Task Combine_2Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2)
	{
		string expectedPath = path1
		                      + FileSystem.Path.DirectorySeparatorChar + path2;

		string result = FileSystem.Path.Combine(path1, path2);

		await That(result).IsEqualTo(expectedPath);
	}

	[Theory]
	[AutoData]
	public async Task Combine_3Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string pathA, string pathB)
	{
		string expectedPath = FileSystem.Path.Combine(pathA, pathB);

		string result1 = FileSystem.Path.Combine(string.Empty, pathA, pathB);
		string result2 = FileSystem.Path.Combine(pathA, string.Empty, pathB);
		string result3 = FileSystem.Path.Combine(pathA, pathB, string.Empty);

		await That(result1).IsEqualTo(expectedPath);
		await That(result2).IsEqualTo(expectedPath);
		await That(result3).IsEqualTo(expectedPath);
	}

	[Theory]
	[AutoData]
	public async Task Combine_3Paths_OneNull_ShouldThrowArgumentNullException(string pathA,
		string pathB)
	{
		void Act1() =>
			FileSystem.Path.Combine(pathA, pathB, null!);

		void Act2() =>
			FileSystem.Path.Combine(null!, pathA, pathB);

		void Act3() =>
			FileSystem.Path.Combine(pathA, null!, pathB);

		await That(Act1).Throws<ArgumentNullException>().WithParamName("path3").And
			.WithHResult(-2147467261);
		await That(Act2).Throws<ArgumentNullException>().WithParamName("path1").And
			.WithHResult(-2147467261);
		await That(Act3).Throws<ArgumentNullException>().WithParamName("path2").And
			.WithHResult(-2147467261);
	}

	[Theory]
	[AutoData]
	public async Task Combine_3Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;
		path3 = FileSystem.Path.DirectorySeparatorChar + path3;

		string result = FileSystem.Path.Combine(path1, path2, path3);

		await That(result).IsEqualTo(path3);
	}

	[Theory]
	[InlineData("", "", "", "")]
	[InlineData("/foo/", "/bar/", "/baz/", "/baz/")]
	[InlineData("foo/", "/bar/", "/baz", "/baz")]
	[InlineData("foo/", "bar", "/baz", "/baz")]
	[InlineData("foo", "/bar", "/baz", "/baz")]
	[InlineData("foo", "/bar/", "baz", "/bar/baz")]
	[InlineData("foo", "bar", "baz", "foo/bar/baz")]
	[InlineData("/foo", "bar", "baz/", "/foo/bar/baz/")]
	public async Task Combine_3Paths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2, path3);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	[InlineAutoData("foo", "bar", " ")]
	public async Task Combine_3Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3)
	{
		string expectedPath = path1
		                      + FileSystem.Path.DirectorySeparatorChar + path2
		                      + FileSystem.Path.DirectorySeparatorChar + path3;

		string result = FileSystem.Path.Combine(path1, path2, path3);

		await That(result).IsEqualTo(expectedPath);
	}

	[Theory]
	[AutoData]
	public async Task Combine_4Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string pathA, string pathB, string pathC)
	{
		string expectedPath = FileSystem.Path.Combine(pathA, pathB, pathC);

		string result1 = FileSystem.Path.Combine(string.Empty, pathA, pathB, pathC);
		string result2 = FileSystem.Path.Combine(pathA, string.Empty, pathB, pathC);
		string result3 = FileSystem.Path.Combine(pathA, pathB, string.Empty, pathC);
		string result4 = FileSystem.Path.Combine(pathA, pathB, pathC, string.Empty);

		await That(result1).IsEqualTo(expectedPath);
		await That(result2).IsEqualTo(expectedPath);
		await That(result3).IsEqualTo(expectedPath);
		await That(result4).IsEqualTo(expectedPath);
	}

	[Theory]
	[AutoData]
	public async Task Combine_4Paths_OneNull_ShouldThrowArgumentNullException(string pathA,
		string pathB,
		string pathC)
	{
		void Act1() =>
			FileSystem.Path.Combine(pathA, pathB, pathC, null!);

		void Act2() =>
			FileSystem.Path.Combine(null!, pathA, pathB, pathC);

		void Act3() =>
			FileSystem.Path.Combine(pathA, null!, pathB, pathC);

		void Act4() =>
			FileSystem.Path.Combine(pathA, pathB, null!, pathC);

		await That(Act1).Throws<ArgumentNullException>().WithParamName("path4").And
			.WithHResult(-2147467261);
		await That(Act2).Throws<ArgumentNullException>().WithParamName("path1").And
			.WithHResult(-2147467261);
		await That(Act3).Throws<ArgumentNullException>().WithParamName("path2").And
			.WithHResult(-2147467261);
		await That(Act4).Throws<ArgumentNullException>().WithParamName("path3").And
			.WithHResult(-2147467261);
	}

	[Theory]
	[AutoData]
	public async Task Combine_4Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3, string path4)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;
		path3 = FileSystem.Path.DirectorySeparatorChar + path3;
		path4 = FileSystem.Path.DirectorySeparatorChar + path4;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4);

		await That(result).IsEqualTo(path4);
	}

	[Theory]
	[InlineData("", "", "", "", "")]
	[InlineData("/foo/", "/bar/", "/baz/", "/muh/", "/muh/")]
	[InlineData("foo/", "/bar/", "/baz/", "/muh", "/muh")]
	[InlineData("foo/", "bar", "/baz", "/muh", "/muh")]
	[InlineData("foo", "/bar", "/baz", "/muh", "/muh")]
	[InlineData("foo", "/bar/", "baz/", "muh", "/bar/baz/muh")]
	[InlineData("foo", "bar", "baz", "muh", "foo/bar/baz/muh")]
	[InlineData("/foo", "bar", "baz", "muh/", "/foo/bar/baz/muh/")]
	public async Task Combine_4Paths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string path4, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path4 = path4.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2, path3, path4);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	[InlineAutoData("foo", "bar", " ")]
	[InlineAutoData("foo", "bar", "baz", " ")]
	public async Task Combine_4Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4)
	{
		string expectedPath = path1
		                      + FileSystem.Path.DirectorySeparatorChar + path2
		                      + FileSystem.Path.DirectorySeparatorChar + path3
		                      + FileSystem.Path.DirectorySeparatorChar + path4;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4);

		await That(result).IsEqualTo(expectedPath);
	}

	[Fact]
	public async Task Combine_ParamPaths_Null_ShouldThrowArgumentNullException()
	{
		void Act() =>
			FileSystem.Path.Combine(null!);

		await That(Act).Throws<ArgumentNullException>().WithParamName("paths").And
			.WithHResult(-2147467261);
	}

	[Theory]
	[AutoData]
	public async Task Combine_ParamPaths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string path1, string path2, string path3, string path4)
	{
		string expectedPath = FileSystem.Path.Combine(path1, path2, path3, path4);

		string result1 =
			FileSystem.Path.Combine(string.Empty, path1, path2, path3, path4);
		string result2 =
			FileSystem.Path.Combine(path1, string.Empty, path2, path3, path4);
		string result3 =
			FileSystem.Path.Combine(path1, path2, string.Empty, path3, path4);
		string result4 =
			FileSystem.Path.Combine(path1, path2, path3, string.Empty, path4);
		string result5 =
			FileSystem.Path.Combine(path1, path2, path3, path4, string.Empty);

		await That(result1).IsEqualTo(expectedPath);
		await That(result2).IsEqualTo(expectedPath);
		await That(result3).IsEqualTo(expectedPath);
		await That(result4).IsEqualTo(expectedPath);
		await That(result5).IsEqualTo(expectedPath);
	}

	[Theory]
	[AutoData]
	public async Task Combine_ParamPaths_OneNull_ShouldThrowArgumentNullException(
		string pathA, string pathB, string pathC, string pathD)
	{
		void Act1() =>
			FileSystem.Path.Combine(pathA, pathB, pathC, pathD, null!);

		void Act2() =>
			FileSystem.Path.Combine(null!, pathA, pathB, pathC, pathD);

		void Act3() =>
			FileSystem.Path.Combine(pathA, null!, pathB, pathC, pathD);

		void Act4() =>
			FileSystem.Path.Combine(pathA, pathB, null!, pathC, pathD);

		void Act5() =>
			FileSystem.Path.Combine(pathA, pathB, pathC, null!, pathD);

		await That(Act1).Throws<ArgumentNullException>().WithParamName("paths").And
			.WithHResult(-2147467261);
		await That(Act2).Throws<ArgumentNullException>().WithParamName("paths").And
			.WithHResult(-2147467261);
		await That(Act3).Throws<ArgumentNullException>().WithParamName("paths").And
			.WithHResult(-2147467261);
		await That(Act4).Throws<ArgumentNullException>().WithParamName("paths").And
			.WithHResult(-2147467261);
		await That(Act5).Throws<ArgumentNullException>().WithParamName("paths").And
			.WithHResult(-2147467261);
	}

	[Theory]
	[AutoData]
	public async Task Combine_ParamPaths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3, string path4, string path5)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;
		path3 = FileSystem.Path.DirectorySeparatorChar + path3;
		path4 = FileSystem.Path.DirectorySeparatorChar + path4;
		path5 = FileSystem.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(path5);
	}

	[Theory]
	[InlineData("", "", "", "", "", "")]
	[InlineData("/foo/", "/bar/", "/baz/", "/muh/", "/maeh/", "/maeh/")]
	[InlineData("foo/", "/bar/", "/baz/", "/muh", "/maeh", "/maeh")]
	[InlineData("foo/", "bar", "/baz", "/muh", "/maeh", "/maeh")]
	[InlineData("foo", "/bar", "/baz", "/muh", "/maeh", "/maeh")]
	[InlineData("foo", "/bar/", "baz/", "muh/", "maeh", "/bar/baz/muh/maeh")]
	[InlineData("foo", "bar", "baz", "muh", "maeh", "foo/bar/baz/muh/maeh")]
	[InlineData("/foo", "bar", "baz", "muh", "maeh/", "/foo/bar/baz/muh/maeh/")]
	public async Task Combine_ParamPaths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string path4, string path5, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path4 = path4.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path5 = path5.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	[InlineAutoData("foo", "bar", " ")]
	[InlineAutoData("foo", "bar", "baz", " ")]
	[InlineAutoData("foo", "bar", "baz", "muh", " ")]
	public async Task Combine_ParamPaths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4, string path5)
	{
		string expectedPath = path1
		                      + FileSystem.Path.DirectorySeparatorChar + path2
		                      + FileSystem.Path.DirectorySeparatorChar + path3
		                      + FileSystem.Path.DirectorySeparatorChar + path4
		                      + FileSystem.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(expectedPath);
	}

#if FEATURE_PATH_SPAN
	[Theory]
	[AutoData]
	public async Task Combine_ReadOnlySpanPaths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string path1, string path2, string path3, string path4)
	{
		string expectedPath = FileSystem.Path.Combine(path1, path2, path3, path4);

		string result1 =
			FileSystem.Path.Combine(string.Empty, path1, path2, path3, path4);
		string result2 =
			FileSystem.Path.Combine(path1, string.Empty, path2, path3, path4);
		string result3 =
			FileSystem.Path.Combine(path1, path2, string.Empty, path3, path4);
		string result4 =
			FileSystem.Path.Combine(path1, path2, path3, string.Empty, path4);
		string result5 =
			FileSystem.Path.Combine(path1, path2, path3, path4, string.Empty);

		await That(result1).IsEqualTo(expectedPath);
		await That(result2).IsEqualTo(expectedPath);
		await That(result3).IsEqualTo(expectedPath);
		await That(result4).IsEqualTo(expectedPath);
		await That(result5).IsEqualTo(expectedPath);
	}

	[Theory]
	[AutoData]
	public async Task Combine_ReadOnlySpanPaths_OneNull_ShouldThrowArgumentNullException(
		string pathA, string pathB, string pathC, string pathD)
	{
		void Act1() =>
			FileSystem.Path.Combine(pathA, pathB, pathC, pathD, null!);
		void Act2() =>
			FileSystem.Path.Combine(null!, pathA, pathB, pathC, pathD);
		void Act3() =>
			FileSystem.Path.Combine(pathA, null!, pathB, pathC, pathD);
		void Act4() =>
			FileSystem.Path.Combine(pathA, pathB, null!, pathC, pathD);
		void Act5() =>
			FileSystem.Path.Combine(pathA, pathB, pathC, null!, pathD);

		await That(Act1).Throws<ArgumentNullException>().WithParamName("paths").And.WithHResult(-2147467261);
		await That(Act2).Throws<ArgumentNullException>().WithParamName("paths").And.WithHResult(-2147467261);
		await That(Act3).Throws<ArgumentNullException>().WithParamName("paths").And.WithHResult(-2147467261);
		await That(Act4).Throws<ArgumentNullException>().WithParamName("paths").And.WithHResult(-2147467261);
		await That(Act5).Throws<ArgumentNullException>().WithParamName("paths").And.WithHResult(-2147467261);
	}

	[Theory]
	[AutoData]
	public async Task Combine_ReadOnlySpanPaths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3, string path4, string path5)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;
		path3 = FileSystem.Path.DirectorySeparatorChar + path3;
		path4 = FileSystem.Path.DirectorySeparatorChar + path4;
		path5 = FileSystem.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(path5);
	}

	[Theory]
	[InlineData("", "", "", "", "", "")]
	[InlineData("/foo/", "/bar/", "/baz/", "/muh/", "/maeh/", "/maeh/")]
	[InlineData("foo/", "/bar/", "/baz/", "/muh", "/maeh", "/maeh")]
	[InlineData("foo/", "bar", "/baz", "/muh", "/maeh", "/maeh")]
	[InlineData("foo", "/bar", "/baz", "/muh", "/maeh", "/maeh")]
	[InlineData("foo", "/bar/", "baz/", "muh/", "maeh", "/bar/baz/muh/maeh")]
	[InlineData("foo", "bar", "baz", "muh", "maeh", "foo/bar/baz/muh/maeh")]
	[InlineData("/foo", "bar", "baz", "muh", "maeh/", "/foo/bar/baz/muh/maeh/")]
	public async Task Combine_ReadOnlySpanPaths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string path4, string path5, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path4 = path4.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path5 = path5.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	[InlineAutoData("foo", "bar", " ")]
	[InlineAutoData("foo", "bar", "baz", " ")]
	[InlineAutoData("foo", "bar", "baz", "muh", " ")]
	public async Task Combine_ReadOnlySpanPaths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4, string path5)
	{
		string expectedPath = path1
							  + FileSystem.Path.DirectorySeparatorChar + path2
							  + FileSystem.Path.DirectorySeparatorChar + path3
							  + FileSystem.Path.DirectorySeparatorChar + path4
							  + FileSystem.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		await That(result).IsEqualTo(expectedPath);
	}
#endif
}
