using System.IO;
using System.Text;
// ReSharper disable StringLiteralTypo

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class SearchFilterTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineAutoData("../", 4)]
	[InlineAutoData("../*", 4)]
	[InlineAutoData("../a*", 2)]
	public void
		SearchPattern_Containing1InstanceOfTwoDotsAndDirectorySeparator_ShouldMatchExpectedFiles(
			string searchPattern, int expectedMatchingFiles)
	{
		Skip.If(Test.IsNetFramework);
		string path = FileSystem.Path.Combine("foo", "bar", "xyz");

		FileSystem.InitializeIn(path)
			.WithFile("test..test")
			.WithFile("a.test")
			.WithFile("a.test.again");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);

		result.Length.Should().Be(expectedMatchingFiles);
		result.Should().Contain(System.IO.Path.Combine(".", "..", "xyz", "a.test"));
	}

	[SkippableTheory]
	[InlineAutoData("../../", 5)]
	[InlineAutoData("../../*", 5)]
	[InlineAutoData("../../a*", 2)]
	public void
		SearchPattern_Containing2InstancesOfMultipleTwoDotsAndDirectorySeparator_ShouldMatchExpectedFiles(
			string searchPattern, int expectedMatchingFiles)
	{
		Skip.If(Test.IsNetFramework);
		string path = FileSystem.Path.Combine("foo", "bar", "xyz");

		FileSystem.InitializeIn(path)
			.WithFile("test..test")
			.WithFile("a.test")
			.WithFile("a.test.again");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);

		result.Length.Should().Be(expectedMatchingFiles);
		if (!searchPattern.EndsWith("a*", StringComparison.Ordinal))
		{
			result.Should().Contain(System.IO.Path.Combine(".", "../..", "bar"));
			result.Should().Contain(System.IO.Path.Combine(".", "../..", "bar", "xyz"));
		}

		result.Should()
			.Contain(System.IO.Path.Combine(".", "../..", "bar", "xyz", "a.test"));
	}

	[SkippableTheory]
	[InlineAutoData("../../../", 6)]
	[InlineAutoData("../../../*", 6)]
	[InlineAutoData("../../../a*", 2)]
	public void
		SearchPattern_Containing3InstancesOfMultipleTwoDotsAndDirectorySeparator_ShouldMatchExpectedFiles(
			string searchPattern, int expectedMatchingFiles)
	{
		Skip.If(Test.IsNetFramework);
		string path = FileSystem.Path.Combine("foo", "bar", "xyz");

		FileSystem.InitializeIn(path)
			.WithFile("test..test")
			.WithFile("a.test")
			.WithFile("a.test.again");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);

		result.Length.Should().Be(expectedMatchingFiles);
		if (!searchPattern.EndsWith("a*", StringComparison.Ordinal))
		{
			result.Should().Contain(System.IO.Path.Combine(".", "../../..", "foo"));
			result.Should()
				.Contain(System.IO.Path.Combine(".", "../../..", "foo", "bar"));
			result.Should()
				.Contain(System.IO.Path.Combine(".", "../../..", "foo", "bar", "xyz"));
		}

		result.Should()
			.Contain(
				System.IO.Path.Combine(".", "../../..", "foo", "bar", "xyz", "a.test"));
	}

	[SkippableFact]
	public void SearchPattern_ContainingAsterisk_ShouldReturnMatchingFiles()
	{
		FileSystem.Initialize()
			.WithFile("a.test")
			.WithFile("a.unmatchingtest")
			.WithFile("another.test")
			.WithFile("a.un-matching-test");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "a*.t*.", SearchOption.AllDirectories);

		result.Length.Should().Be(2);
		result.Should().Contain(System.IO.Path.Combine(".", "a.test"));
		result.Should().Contain(System.IO.Path.Combine(".", "another.test"));
	}

	[SkippableFact]
	public void SearchPattern_ContainingQuestionMark_ShouldReturnMatchingFiles()
	{
		FileSystem.Initialize()
			.WithFile("a-test")
			.WithFile("a-unmatchingtest")
			.WithFile("another-test")
			.WithFile("a-un-matching-test");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "a-??s*", SearchOption.AllDirectories);

		result.Length.Should().Be(1);
		result[0].Should().Be(System.IO.Path.Combine(".", "a-test"));
	}

	[SkippableFact]
	public void
		SearchPattern_ContainingTooManyInstancesOfMultipleTwoDotsAndDirectorySeparator_ShouldThrowUnauthorizedAccessException()
	{
		Skip.If(Test.IsNetFramework);

		FileSystem.Initialize()
			.WithFile("a.txt");

		string currentDirectory = FileSystem.Directory.GetCurrentDirectory();
		int directoryCount = currentDirectory.Length -
		                     currentDirectory
			                     .Replace($"{FileSystem.Path.DirectorySeparatorChar}", "",
				                     StringComparison.Ordinal)
			                     .Length;

		StringBuilder sb = new();
		for (int i = 0; i <= directoryCount; i++)
		{
			sb.Append("../");
		}

		sb.Append("a*");
		string searchPattern = sb.ToString();

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory
				.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}

	[SkippableTheory]
	[InlineAutoData("../", 4)]
	[InlineAutoData("../*", 4)]
	[InlineAutoData("../a*", 2)]
	public void
		SearchPattern_ContainingTwoDotsAndDirectorySeparator_ShouldMatchExpectedFiles(
			string searchPattern, int expectedMatchingFiles, string path)
	{
		Skip.If(Test.IsNetFramework);

		FileSystem.InitializeIn(path)
			.WithFile("test..test")
			.WithFile("a.test")
			.WithFile("a.test.again");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);

		result.Length.Should().Be(expectedMatchingFiles);
		result.Should().Contain(System.IO.Path.Combine(".", "..", path, "a.test"));
	}

	[SkippableTheory]
	[InlineAutoData("../")]
	[InlineAutoData("../*")]
	[InlineAutoData("../a*")]
	[InlineAutoData("*t..")]
	public void
		SearchPattern_ContainingTwoDotsAndDirectorySeparator_ShouldThrowArgumentException_OnNetFramework(
			string searchPattern, string path)
	{
		Skip.IfNot(Test.IsNetFramework);

		FileSystem.InitializeIn(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory
				.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[SkippableFact]
	public void SearchPattern_ContainingWithTwoDots_ShouldContainMatchingFiles()
	{
		FileSystem.Initialize()
			.WithFile("test..x")
			.WithFile("a.test...x")
			.WithFile("a.test.again..x");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "*t..x", SearchOption.AllDirectories);

		result.Length.Should().Be(1);
	}

	[SkippableFact]
	public void SearchPattern_EndingWithTwoDots_ShouldNotMatchAnyFile()
	{
		Skip.If(Test.IsNetFramework);

		FileSystem.Initialize()
			.WithFile("test..")
			.WithFile("a.test...")
			.WithFile("a.test.again..");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "*t..", SearchOption.AllDirectories);

		if (Test.RunsOnWindows)
		{
			result.Should().BeEmpty();
		}
		else
		{
			result.Length.Should().Be(1);
			result.Should().Contain(System.IO.Path.Combine(".", "test.."));
		}
	}

	[SkippableFact]
	public void SearchPattern_Extension_ShouldReturnAllFilesWithTheExtension()
	{
		FileSystem.Initialize()
			.WithAFile(".gif")
			.WithAFile(".jpg")
			.WithASubdirectory().Initialized(s => s
				.WithAFile(".gif")
				.WithASubdirectory().Initialized(t => t
					.WithAFile(".gif")
					.WithFile("a.gif.txt")));

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "*.gif", SearchOption.AllDirectories);

		result.Length.Should().Be(3);
	}

	[SkippableFact]
	public void SearchPattern_Null_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory
				.GetFileSystemEntries(".", null!, SearchOption.AllDirectories);
		});

		exception.Should().BeException<ArgumentNullException>(paramName: "searchPattern");
	}

	[SkippableFact]
	public void SearchPattern_StarDot_ShouldReturnFilesWithoutExtension()
	{
		FileSystem.Initialize()
			.WithFile("test.")
			.WithFile("a.test.")
			.WithFile("a.test.again.");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "*.", SearchOption.AllDirectories);

		if (Test.RunsOnWindows)
		{
			result.Length.Should().Be(1);
			result.Should().Contain(System.IO.Path.Combine(".", "test"));
		}
		else
		{
			result.Length.Should().Be(3);
			result.Should().Contain(System.IO.Path.Combine(".", "test."));
		}
	}

	[SkippableTheory]
#if NETFRAMEWORK
	[InlineAutoData(false, "")]
#else
	[InlineAutoData(true, "")]
#endif
	[InlineAutoData(true, "*")]
	[InlineAutoData(true, ".")]
	[InlineAutoData(true, "*.*")]
	public void SearchPattern_WildCard_ShouldReturnFile(
		bool expectToBeFound, string searchPattern, string path)
	{
		FileSystem.Initialize().WithFile(path);

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", searchPattern);

		if (expectToBeFound)
		{
			result.Should().ContainSingle(
				path,
				$"{searchPattern} should match any path.");
		}
		else
		{
			result.Should()
				.BeEmpty($"{searchPattern} should not match {path}");
		}
	}
}
