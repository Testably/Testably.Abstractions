namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CombineTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Combine_2Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string path)
	{
		string result1 = FileSystem.Path.Combine(path, string.Empty);
		string result2 = FileSystem.Path.Combine(string.Empty, path);

		result1.Should().Be(path);
		result2.Should().Be(path);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_2Paths_OneNull_ShouldThrowArgumentNullException(string path)
	{
		Exception? exception1 = Record.Exception(() =>
			FileSystem.Path.Combine(path, null!));
		Exception? exception2 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, path));

		exception1.Should()
			.BeException<ArgumentNullException>(paramName: "path2", hResult: -2147467261);
		exception2.Should()
			.BeException<ArgumentNullException>(paramName: "path1", hResult: -2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_2Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;

		string result = FileSystem.Path.Combine(path1, path2);

		result.Should().Be(path2);
	}

	[SkippableTheory]
	[InlineAutoData("/foo/", "/bar/", "/bar/")]
	[InlineAutoData("foo/", "/bar", "/bar")]
	[InlineAutoData("foo/", "bar", "foo/bar")]
	[InlineAutoData("foo", "/bar", "/bar")]
	[InlineAutoData("foo", "bar", "foo/bar")]
	[InlineAutoData("/foo", "bar/", "/foo/bar/")]
	public void Combine_2Paths_ShouldReturnExpectedResult(
		string path1, string path2, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2);

		result.Should().Be(expectedResult);
	}

	[SkippableTheory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	public void Combine_2Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2)
	{
		string expectedPath = path1
		                      + FileSystem.Path.DirectorySeparatorChar + path2;

		string result = FileSystem.Path.Combine(path1, path2);

		result.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_3Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string pathA, string pathB)
	{
		string expectedPath = FileSystem.Path.Combine(pathA, pathB);

		string result1 = FileSystem.Path.Combine(string.Empty, pathA, pathB);
		string result2 = FileSystem.Path.Combine(pathA, string.Empty, pathB);
		string result3 = FileSystem.Path.Combine(pathA, pathB, string.Empty);

		result1.Should().Be(expectedPath);
		result2.Should().Be(expectedPath);
		result3.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_3Paths_OneNull_ShouldThrowArgumentNullException(string pathA, string pathB)
	{
		Exception? exception1 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, pathB, null!));
		Exception? exception2 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, pathA, pathB));
		Exception? exception3 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, null!, pathB));

		exception1.Should()
			.BeException<ArgumentNullException>(paramName: "path3", hResult: -2147467261);
		exception2.Should()
			.BeException<ArgumentNullException>(paramName: "path1", hResult: -2147467261);
		exception3.Should()
			.BeException<ArgumentNullException>(paramName: "path2", hResult: -2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_3Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;
		path3 = FileSystem.Path.DirectorySeparatorChar + path3;

		string result = FileSystem.Path.Combine(path1, path2, path3);

		result.Should().Be(path3);
	}

	[SkippableTheory]
	[InlineAutoData("/foo/", "/bar/", "/baz/", "/baz/")]
	[InlineAutoData("foo/", "/bar/", "/baz", "/baz")]
	[InlineAutoData("foo/", "bar", "/baz", "/baz")]
	[InlineAutoData("foo", "/bar", "/baz", "/baz")]
	[InlineAutoData("foo", "/bar/", "baz", "/bar/baz")]
	[InlineAutoData("foo", "bar", "baz", "foo/bar/baz")]
	[InlineAutoData("/foo", "bar", "baz/", "/foo/bar/baz/")]
	public void Combine_3Paths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2, path3);

		result.Should().Be(expectedResult);
	}

	[SkippableTheory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	[InlineAutoData("foo", "bar", " ")]
	public void Combine_3Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3)
	{
		string expectedPath = path1
		                      + FileSystem.Path.DirectorySeparatorChar + path2
		                      + FileSystem.Path.DirectorySeparatorChar + path3;

		string result = FileSystem.Path.Combine(path1, path2, path3);

		result.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_4Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string pathA, string pathB, string pathC)
	{
		string expectedPath = FileSystem.Path.Combine(pathA, pathB, pathC);

		string result1 = FileSystem.Path.Combine(string.Empty, pathA, pathB, pathC);
		string result2 = FileSystem.Path.Combine(pathA, string.Empty, pathB, pathC);
		string result3 = FileSystem.Path.Combine(pathA, pathB, string.Empty, pathC);
		string result4 = FileSystem.Path.Combine(pathA, pathB, pathC, string.Empty);

		result1.Should().Be(expectedPath);
		result2.Should().Be(expectedPath);
		result3.Should().Be(expectedPath);
		result4.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_4Paths_OneNull_ShouldThrowArgumentNullException(string pathA, string pathB,
		string pathC)
	{
		Exception? exception1 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, pathB, pathC, null!));
		Exception? exception2 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, pathA, pathB, pathC));
		Exception? exception3 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, null!, pathB, pathC));
		Exception? exception4 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, pathB, null!, pathC));

		exception1.Should()
			.BeException<ArgumentNullException>(paramName: "path4", hResult: -2147467261);
		exception2.Should()
			.BeException<ArgumentNullException>(paramName: "path1", hResult: -2147467261);
		exception3.Should()
			.BeException<ArgumentNullException>(paramName: "path2", hResult: -2147467261);
		exception4.Should()
			.BeException<ArgumentNullException>(paramName: "path3", hResult: -2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_4Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3, string path4)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;
		path3 = FileSystem.Path.DirectorySeparatorChar + path3;
		path4 = FileSystem.Path.DirectorySeparatorChar + path4;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4);

		result.Should().Be(path4);
	}

	[SkippableTheory]
	[InlineAutoData("/foo/", "/bar/", "/baz/", "/muh/", "/muh/")]
	[InlineAutoData("foo/", "/bar/", "/baz/", "/muh", "/muh")]
	[InlineAutoData("foo/", "bar", "/baz", "/muh", "/muh")]
	[InlineAutoData("foo", "/bar", "/baz", "/muh", "/muh")]
	[InlineAutoData("foo", "/bar/", "baz/", "muh", "/bar/baz/muh")]
	[InlineAutoData("foo", "bar", "baz", "muh", "foo/bar/baz/muh")]
	[InlineAutoData("/foo", "bar", "baz", "muh/", "/foo/bar/baz/muh/")]
	public void Combine_4Paths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string path4, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path4 = path4.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2, path3, path4);

		result.Should().Be(expectedResult);
	}

	[SkippableTheory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	[InlineAutoData("foo", "bar", " ")]
	[InlineAutoData("foo", "bar", "baz", " ")]
	public void Combine_4Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4)
	{
		string expectedPath = path1
		                      + FileSystem.Path.DirectorySeparatorChar + path2
		                      + FileSystem.Path.DirectorySeparatorChar + path3
		                      + FileSystem.Path.DirectorySeparatorChar + path4;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4);

		result.Should().Be(expectedPath);
	}

	[SkippableFact]
	public void Combine_ParamPaths_Null_ShouldThrowArgumentNullException()
	{
		Exception? exception = Record.Exception(() =>
			FileSystem.Path.Combine(null!));

		exception.Should()
			.BeException<ArgumentNullException>(paramName: "paths", hResult: -2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_ParamPaths_OneEmpty_ShouldReturnCombinationOfOtherParts(
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

		result1.Should().Be(expectedPath);
		result2.Should().Be(expectedPath);
		result3.Should().Be(expectedPath);
		result4.Should().Be(expectedPath);
		result5.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_ParamPaths_OneNull_ShouldThrowArgumentNullException(
		string pathA, string pathB, string pathC, string pathD)
	{
		Exception? exception1 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, pathB, pathC, pathD, null!));
		Exception? exception2 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, pathA, pathB, pathC, pathD));
		Exception? exception3 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, null!, pathB, pathC, pathD));
		Exception? exception4 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, pathB, null!, pathC, pathD));
		Exception? exception5 = Record.Exception(() =>
			FileSystem.Path.Combine(pathA, pathB, pathC, null!, pathD));

		exception1.Should()
			.BeException<ArgumentNullException>(paramName: "paths", hResult: -2147467261);
		exception2.Should()
			.BeException<ArgumentNullException>(paramName: "paths", hResult: -2147467261);
		exception3.Should()
			.BeException<ArgumentNullException>(paramName: "paths", hResult: -2147467261);
		exception4.Should()
			.BeException<ArgumentNullException>(paramName: "paths", hResult: -2147467261);
		exception5.Should()
			.BeException<ArgumentNullException>(paramName: "paths", hResult: -2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_ParamPaths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3, string path4, string path5)
	{
		path1 = FileSystem.Path.DirectorySeparatorChar + path1;
		path2 = FileSystem.Path.DirectorySeparatorChar + path2;
		path3 = FileSystem.Path.DirectorySeparatorChar + path3;
		path4 = FileSystem.Path.DirectorySeparatorChar + path4;
		path5 = FileSystem.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		result.Should().Be(path5);
	}

	[SkippableTheory]
	[InlineAutoData("/foo/", "/bar/", "/baz/", "/muh/", "/maeh/", "/maeh/")]
	[InlineAutoData("foo/", "/bar/", "/baz/", "/muh", "/maeh", "/maeh")]
	[InlineAutoData("foo/", "bar", "/baz", "/muh", "/maeh", "/maeh")]
	[InlineAutoData("foo", "/bar", "/baz", "/muh", "/maeh", "/maeh")]
	[InlineAutoData("foo", "/bar/", "baz/", "muh/", "maeh", "/bar/baz/muh/maeh")]
	[InlineAutoData("foo", "bar", "baz", "muh", "maeh", "foo/bar/baz/muh/maeh")]
	[InlineAutoData("/foo", "bar", "baz", "muh", "maeh/", "/foo/bar/baz/muh/maeh/")]
	public void Combine_ParamPaths_ShouldReturnExpectedResult(
		string path1, string path2, string path3, string path4, string path5, string expectedResult)
	{
		path1 = path1.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path2 = path2.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path3 = path3.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path4 = path4.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		path5 = path5.Replace('/', FileSystem.Path.DirectorySeparatorChar);
		expectedResult = expectedResult.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		result.Should().Be(expectedResult);
	}

	[SkippableTheory]
	[InlineAutoData]
	[InlineAutoData(" ")]
	[InlineAutoData("foo", " ")]
	[InlineAutoData("foo", "bar", " ")]
	[InlineAutoData("foo", "bar", "baz", " ")]
	[InlineAutoData("foo", "bar", "baz", "muh", " ")]
	public void Combine_ParamPaths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
		string path1, string path2, string path3, string path4, string path5)
	{
		string expectedPath = path1
		                      + FileSystem.Path.DirectorySeparatorChar + path2
		                      + FileSystem.Path.DirectorySeparatorChar + path3
		                      + FileSystem.Path.DirectorySeparatorChar + path4
		                      + FileSystem.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		result.Should().Be(expectedPath);
	}
}
