namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ExistsTests
{
	[SkippableTheory]
	[AutoData]
	public void Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.File.Exists(path);

		result.Should().BeFalse();
	}

	[SkippableFact]
	public void Exists_Empty_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(string.Empty);

		result.Should().BeFalse();
	}

	[SkippableFact]
	public void Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(null);

		result.Should().BeFalse();
	}
}
