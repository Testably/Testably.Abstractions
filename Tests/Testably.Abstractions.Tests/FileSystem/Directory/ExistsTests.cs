namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public async Task Exists_ExistingDirectory_ShouldReturnTrue(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task Exists_ForwardSlash_ShouldReturnTrue()
	{
		FileSystem.InitializeIn("D:");

		bool result = FileSystem.Directory.Exists("/");

		await That(result).IsTrue();
	}

	[Fact]
	public async Task Exists_ForwardSlashWithDirectory_ShouldReturnTrue()
	{
		FileSystem.Directory.CreateDirectory("/tmp");

		bool result = FileSystem.Directory.Exists("/tmp");

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(@"\\s")]
	[InlineData("<")]
	[InlineData("\t")]
	public async Task Exists_IllegalPath_ShouldReturnFalse(string path)
	{
		Skip.If(Test.IsNetFramework);

		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsFalse();
	}

	[Theory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public async Task Exists_MissingDirectory_ShouldReturnFalse(string path)
	{
		bool result = FileSystem.Directory.Exists(path);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists(null);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task Exists_Whitespace_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists("  ");

		await That(result).IsFalse();
	}
}
