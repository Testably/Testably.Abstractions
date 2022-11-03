using System;

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

		exception1.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception2.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_2Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2)
	{
		path1 = System.IO.Path.DirectorySeparatorChar + path1;
		path2 = System.IO.Path.DirectorySeparatorChar + path2;

		string result = FileSystem.Path.Combine(path1, path2);

		result.Should().Be(path2);
	}

	[SkippableTheory]
	[AutoData]
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
		string path1, string path2)
	{
		string expectedPath = System.IO.Path.Combine(path1, path2);

		string result1 = FileSystem.Path.Combine(string.Empty, path1, path2);
		string result2 = FileSystem.Path.Combine(path1, string.Empty, path2);
		string result3 = FileSystem.Path.Combine(path1, path2, string.Empty);

		result1.Should().Be(expectedPath);
		result2.Should().Be(expectedPath);
		result3.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_3Paths_OneNull_ShouldThrowArgumentNullException(string path)
	{
		Exception? exception1 = Record.Exception(() =>
			FileSystem.Path.Combine(path, null!, null!));
		Exception? exception2 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, path, null!));
		Exception? exception3 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, null!, path));

		exception1.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception2.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception3.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_3Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3)
	{
		path1 = System.IO.Path.AltDirectorySeparatorChar + path1;
		path2 = System.IO.Path.AltDirectorySeparatorChar + path2;
		path3 = System.IO.Path.AltDirectorySeparatorChar + path3;

		string result = FileSystem.Path.Combine(path1, path2, path3);

		result.Should().Be(path3);
	}

	[SkippableTheory]
	[AutoData]
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
		string path1, string path2, string path3)
	{
		string expectedPath = System.IO.Path.Combine(path1, path2, path3);

		string result1 = FileSystem.Path.Combine(string.Empty, path1, path2, path3);
		string result2 = FileSystem.Path.Combine(path1, string.Empty, path2, path3);
		string result3 = FileSystem.Path.Combine(path1, path2, string.Empty, path3);
		string result4 = FileSystem.Path.Combine(path1, path2, path3, string.Empty);

		result1.Should().Be(expectedPath);
		result2.Should().Be(expectedPath);
		result3.Should().Be(expectedPath);
		result4.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_4Paths_OneNull_ShouldThrowArgumentNullException(string path)
	{
		Exception? exception1 = Record.Exception(() =>
			FileSystem.Path.Combine(path, null!, null!, null!));
		Exception? exception2 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, path, null!, null!));
		Exception? exception3 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, null!, path, null!));
		Exception? exception4 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, null!, null!, path));

		exception1.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception2.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception3.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception4.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_4Paths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3, string path4)
	{
		path1 = System.IO.Path.DirectorySeparatorChar + path1;
		path2 = System.IO.Path.DirectorySeparatorChar + path2;
		path3 = System.IO.Path.DirectorySeparatorChar + path3;
		path4 = System.IO.Path.DirectorySeparatorChar + path4;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4);

		result.Should().Be(path4);
	}

	[SkippableTheory]
	[AutoData]
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

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_ParamPaths_OneEmpty_ShouldReturnCombinationOfOtherParts(
		string path1, string path2, string path3, string path4)
	{
		string expectedPath = System.IO.Path.Combine(path1, path2, path3, path4);

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
		string path)
	{
		Exception? exception1 = Record.Exception(() =>
			FileSystem.Path.Combine(path, null!, null!, null!, null!));
		Exception? exception2 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, path, null!, null!, null!));
		Exception? exception3 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, null!, path, null!, null!));
		Exception? exception4 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, null!, null!, path, null!));
		Exception? exception5 = Record.Exception(() =>
			FileSystem.Path.Combine(null!, null!, null!, null!, path));

		exception1.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception2.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception3.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception4.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
		exception5.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void Combine_ParamPaths_Rooted_ShouldReturnLastRootedPath(
		string path1, string path2, string path3, string path4, string path5)
	{
		path1 = System.IO.Path.DirectorySeparatorChar + path1;
		path2 = System.IO.Path.DirectorySeparatorChar + path2;
		path3 = System.IO.Path.DirectorySeparatorChar + path3;
		path4 = System.IO.Path.DirectorySeparatorChar + path4;
		path5 = System.IO.Path.DirectorySeparatorChar + path5;

		string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

		result.Should().Be(path5);
	}

	[SkippableTheory]
	[AutoData]
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