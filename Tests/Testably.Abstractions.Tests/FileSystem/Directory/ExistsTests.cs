namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public void Exists_ExistingDirectory_ShouldReturnTrue(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Directory.Exists(path);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		bool result = FileSystem.Directory.Exists(path);

		result.Should().BeFalse();
	}

	[Fact]
	public void Exists_ForwardSlash_ShouldReturnTrue()
	{
		FileSystem.InitializeIn("D:");
		
		bool result = FileSystem.Directory.Exists("/");

		result.Should().BeTrue();
	}

	[Fact]
	public void Exists_ForwardSlashWithDirectory_ShouldReturnTrue()
	{
		FileSystem.Directory.CreateDirectory("/tmp");
		
		bool result = FileSystem.Directory.Exists("/tmp");

		result.Should().BeTrue();
	}

	[Theory]
	[InlineData(@"\\s")]
	[InlineData("<")]
	[InlineData("\t")]
	public void Exists_IllegalPath_ShouldReturnFalse(string path)
	{
		Skip.If(Test.IsNetFramework);

		bool result = FileSystem.Directory.Exists(path);

		result.Should().BeFalse();
	}

	[Theory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public void Exists_MissingDirectory_ShouldReturnFalse(string path)
	{
		bool result = FileSystem.Directory.Exists(path);

		result.Should().BeFalse();
	}

	[Fact]
	public void Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists(null);

		result.Should().BeFalse();
	}

	[Fact]
	public void Exists_Whitespace_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists("  ");

		result.Should().BeFalse();
	}
}
