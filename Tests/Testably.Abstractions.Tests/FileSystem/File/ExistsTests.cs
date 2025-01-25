namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[AutoData]
	public void Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.File.Exists(path);

		result.Should().BeFalse();
	}

	[Fact]
	public void Exists_Empty_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(string.Empty);

		result.Should().BeFalse();
	}

	[Fact]
	public void Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(null);

		result.Should().BeFalse();
	}
}
