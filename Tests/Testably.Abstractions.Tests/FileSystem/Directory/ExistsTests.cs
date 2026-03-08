namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public class ExistsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[Arguments("foo")]
	[Arguments("foo/")]
	public async Task Exists_ExistingDirectory_ShouldReturnTrue(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsFalse();
	}

	[Test]
	public async Task Exists_ForwardSlash_ShouldReturnTrue()
	{
		FileSystem.InitializeIn("/");

		bool result = FileSystem.Directory.Exists("/");

		await That(result).IsTrue();
	}

	[Test]
	public async Task Exists_ForwardSlashWithDirectory_ShouldReturnTrue()
	{
		FileSystem.Directory.CreateDirectory("/tmp");

		bool result = FileSystem.Directory.Exists("/tmp");

		await That(result).IsTrue();
	}

	[Test]
	[Arguments(@"\\s")]
	[Arguments("<")]
	[Arguments("\t")]
	public async Task Exists_IllegalPath_ShouldReturnFalse(string path)
	{
		Skip.If(Test.IsNetFramework);

		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsFalse();
	}

	[Test]
	[Arguments("foo")]
	[Arguments("foo/")]
	public async Task Exists_MissingDirectory_ShouldReturnFalse(string path)
	{
		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsFalse();
	}

	[Test]
	public async Task Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists(null);

		await That(result).IsFalse();
	}

	[Test]
	public async Task Exists_Whitespace_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists("  ");

		await That(result).IsFalse();
	}
}
