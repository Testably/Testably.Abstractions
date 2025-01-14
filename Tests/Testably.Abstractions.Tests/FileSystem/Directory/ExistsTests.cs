namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class ExistsTests
{
	[SkippableTheory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public void Exists_ExistingDirectory_ShouldReturnTrue(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Directory.Exists(path);

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		bool result = FileSystem.Directory.Exists(path);

		result.Should().BeFalse();
	}

	[SkippableTheory]
	[InlineData(@"\\s")]
	[InlineData("<")]
	[InlineData("\t")]
	public void Exists_IllegalPath_ShouldReturnFalse(string path)
	{
		Skip.If(Test.IsNetFramework);

		bool result = FileSystem.Directory.Exists(path);

		result.Should().BeFalse();
	}

	[SkippableTheory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public void Exists_MissingDirectory_ShouldReturnFalse(string path)
	{
		bool result = FileSystem.Directory.Exists(path);

		result.Should().BeFalse();
	}

	[SkippableFact]
	public void Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists(null);

		result.Should().BeFalse();
	}

	[SkippableFact]
	public void Exists_Whitespace_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists("  ");

		result.Should().BeFalse();
	}
}
