namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[AutoData]
	public async Task Exists_ArbitraryPath_ShouldBeFalse(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		await That(sut.Exists).IsFalse();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Exists_ExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();
		FileSystem.Directory.Delete(path);

		await That(sut.Exists).IsTrue();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		await That(sut.Exists).IsFalse();
	}

	[Fact]
	public async Task Exists_ForwardSlash_ShouldReturnTrue()
	{
		FileSystem.InitializeIn("/");

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("/");

		await That(sut.Exists).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Exists_NotExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsFalse();
		FileSystem.Directory.CreateDirectory(path);

		await That(sut.Exists).IsFalse();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Exists_ShouldNotChangeOnMoveTo(string path, string destination)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.MoveTo(destination);

		await That(sut.Exists).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Exists_ShouldUpdateOnCreateWhenNotNetFramework(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsFalse();

		sut.Create();

		await That(sut.Exists).IsEqualTo(!Test.IsNetFramework);
	}

	[Theory]
	[AutoData]
	public async Task Exists_ShouldUpdateOnDeleteWhenNotNetFramework(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.Delete();

		await That(sut.Exists).IsEqualTo(Test.IsNetFramework);
	}

	[Theory]
	[AutoData]
	public async Task Exists_ShouldUpdateOnRecursiveDeleteWhenNotNetFramework(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.Delete(true);

		await That(sut.Exists).IsEqualTo(Test.IsNetFramework);
	}

	[Theory]
	[AutoData]
	public async Task Exists_ShouldUpdateOnRefresh(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();
		FileSystem.Directory.Delete(path);
		await That(sut.Exists).IsTrue();

		sut.Refresh();

		await That(sut.Exists).IsFalse();
	}
}
