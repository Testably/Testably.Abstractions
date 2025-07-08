namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[AutoData]
	public async Task Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.File.Exists(path);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task Exists_Empty_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(string.Empty);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(null);

		await That(result).IsFalse();
	}
}
