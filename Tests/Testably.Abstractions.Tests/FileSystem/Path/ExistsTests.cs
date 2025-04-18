#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[AutoData]
	public void Exists_ExistingDirectory_ShouldReturnTrue(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Path.Exists(path);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Exists_ExistingFile_ShouldReturnTrue(string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		bool result = FileSystem.Path.Exists(path);

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Exists_ExistingFileOrDirectory_ShouldReturnTrue(string path)
	{
		bool result = FileSystem.Path.Exists(path);

		result.Should().BeFalse();
	}

	[Fact]
	public void Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Path.Exists(null);

		result.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Exists_ShouldWorkWithAbsolutePaths(string path)
	{
		string absolutePath = FileSystem.Path.GetFullPath(path);
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Path.Exists(absolutePath);

		result.Should().BeTrue();
	}
}
#endif
