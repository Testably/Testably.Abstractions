#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[AutoData]
	public async Task Exists_ExistingDirectory_ShouldReturnTrue(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Path.Exists(path);

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Exists_ExistingFile_ShouldReturnTrue(string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		bool result = FileSystem.Path.Exists(path);

		await That(result).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Exists_ExistingFileOrDirectory_ShouldReturnTrue(string path)
	{
		bool result = FileSystem.Path.Exists(path);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Path.Exists(null);

		await That(result).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Exists_ShouldWorkWithAbsolutePaths(string path)
	{
		string absolutePath = FileSystem.Path.GetFullPath(path);
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Path.Exists(absolutePath);

		await That(result).IsTrue();
	}
}
#endif
