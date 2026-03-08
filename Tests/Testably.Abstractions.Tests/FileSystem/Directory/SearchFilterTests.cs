using System.IO;
using System.Text;
// ReSharper disable StringLiteralTypo

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public class SearchFilterTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments("../", 4)]
	[AutoArguments("../*", 4)]
	[AutoArguments("../a*", 2)]
	public async Task
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

		await That(result.Length).IsEqualTo(expectedMatchingFiles);
		await That(result).Contains(FileSystem.Path.Combine(".", "..", "xyz", "a.test"));
	}

	[Test]
	[AutoArguments("../../", 5)]
	[AutoArguments("../../*", 5)]
	[AutoArguments("../../a*", 2)]
	public async Task
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

		await That(result.Length).IsEqualTo(expectedMatchingFiles);
		if (!searchPattern.EndsWith("a*", StringComparison.Ordinal))
		{
			await That(result).Contains(FileSystem.Path.Combine(".", "../..", "bar"));
			await That(result).Contains(FileSystem.Path.Combine(".", "../..", "bar", "xyz"));
		}

		await That(result).Contains(FileSystem.Path.Combine(".", "../..", "bar", "xyz", "a.test"));
	}

	[Test]
	[AutoArguments("../../../", 6)]
	[AutoArguments("../../../*", 6)]
	[AutoArguments("../../../a*", 2)]
	public async Task
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

		await That(result.Length).IsEqualTo(expectedMatchingFiles);
		if (!searchPattern.EndsWith("a*", StringComparison.Ordinal))
		{
			await That(result).Contains(FileSystem.Path.Combine(".", "../../..", "foo"));
			await That(result).Contains(FileSystem.Path.Combine(".", "../../..", "foo", "bar"));
			await That(result)
				.Contains(FileSystem.Path.Combine(".", "../../..", "foo", "bar", "xyz"));
		}

		await That(result)
			.Contains(FileSystem.Path.Combine(".", "../../..", "foo", "bar", "xyz", "a.test"));
	}

	[Test]
	public async Task SearchPattern_ContainingAsterisk_ShouldReturnMatchingFiles()
	{
		FileSystem.Initialize()
			.WithFile("a.test")
			.WithFile("a.unmatchingtest")
			.WithFile("another.test")
			.WithFile("a.un-matching-test");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "a*.t*.", SearchOption.AllDirectories);

		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(FileSystem.Path.Combine(".", "a.test"));
		await That(result).Contains(FileSystem.Path.Combine(".", "another.test"));
	}

	[Test]
	public async Task SearchPattern_ContainingQuestionMark_ShouldReturnMatchingFiles()
	{
		FileSystem.Initialize()
			.WithFile("a-test")
			.WithFile("a-unmatchingtest")
			.WithFile("another-test")
			.WithFile("a-un-matching-test");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "a-??s*", SearchOption.AllDirectories);

		await That(result.Length).IsEqualTo(1);
		await That(result[0]).IsEqualTo(FileSystem.Path.Combine(".", "a-test"));
	}

	[Test]
	public async Task
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

		void Act()
		{
			FileSystem.Directory
				.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Test]
	[Arguments("../", 4, "my-path")]
	[Arguments("../*", 4, "my-path")]
	[Arguments("../a*", 2, "my-path")]
	public async Task SearchPattern_ContainingTwoDotsAndDirectorySeparator_ShouldMatchExpectedFiles(
		string searchPattern, int expectedMatchingFiles, string path)
	{
		Skip.If(Test.IsNetFramework);

		FileSystem.InitializeIn(path)
			.WithFile("test..test")
			.WithFile("a.test")
			.WithFile("a.test.again");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);

		await That(result.Length).IsEqualTo(expectedMatchingFiles);
		await That(result).Contains(FileSystem.Path.Combine(".", "..", path, "a.test"));
	}

	[Test]
	[AutoArguments("../")]
	[AutoArguments("../*")]
	[AutoArguments("../a*")]
	[AutoArguments("*t..")]
	public async Task
		SearchPattern_ContainingTwoDotsAndDirectorySeparator_ShouldThrowArgumentException_OnNetFramework(
			string searchPattern, string path)
	{
		Skip.IfNot(Test.IsNetFramework);

		FileSystem.InitializeIn(path);

		void Act()
		{
			FileSystem.Directory
				.GetFileSystemEntries(".", searchPattern, SearchOption.AllDirectories);
		}

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Test]
	public async Task SearchPattern_ContainingWithTwoDots_ShouldContainMatchingFiles()
	{
		FileSystem.Initialize()
			.WithFile("test..x")
			.WithFile("a.test...x")
			.WithFile("a.test.again..x");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "*t..x", SearchOption.AllDirectories);

		await That(result.Length).IsEqualTo(1);
	}

	[Test]
	public async Task SearchPattern_EndingWithTwoDots_ShouldNotMatchAnyFile()
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
			await That(result).IsEmpty();
		}
		else
		{
			await That(result.Length).IsEqualTo(1);
			await That(result).Contains(FileSystem.Path.Combine(".", "test.."));
		}
	}

	[Test]
	public async Task SearchPattern_Extension_ShouldReturnAllFilesWithTheExtension()
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

		await That(result.Length).IsEqualTo(3);
	}

	[Test]
	public async Task SearchPattern_Null_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();

		void Act()
		{
			FileSystem.Directory
				.GetFileSystemEntries(".", null!, SearchOption.AllDirectories);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("searchPattern");
	}

	[Test]
	public async Task SearchPattern_StarDot_ShouldReturnFilesWithoutExtension()
	{
		FileSystem.Initialize()
			.WithFile("test.")
			.WithFile("a.test.")
			.WithFile("a.test.again.");

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", "*.", SearchOption.AllDirectories);

		if (Test.RunsOnWindows)
		{
			await That(result.Length).IsEqualTo(1);
			await That(result).Contains(FileSystem.Path.Combine(".", "test"));
		}
		else
		{
			await That(result.Length).IsEqualTo(3);
			await That(result).Contains(FileSystem.Path.Combine(".", "test."));
		}
	}

	[Test]
#if NETFRAMEWORK
	[AutoArguments(false, "")]
#else
	[AutoArguments(true, "")]
#endif
	[AutoArguments(true, "*")]
	[AutoArguments(true, ".")]
	[AutoArguments(true, "*.*")]
	public async Task SearchPattern_WildCard_ShouldReturnFile(
		bool expectToBeFound, string searchPattern, string path)
	{
		FileSystem.Initialize().WithFile(path);

		string[] result = FileSystem.Directory
			.GetFileSystemEntries(".", searchPattern);

		if (expectToBeFound)
		{
			await That(result).HasSingle().Which.EndsWith(path)
				.Because($"it should match {searchPattern}");
		}
		else
		{
			await That(result).IsEmpty().Because($"{searchPattern} should not match {path}");
		}
	}
}
