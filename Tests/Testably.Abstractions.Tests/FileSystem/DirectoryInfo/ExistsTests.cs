namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public class ExistsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Exists_ArbitraryPath_ShouldBeFalse(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		await That(sut.Exists).IsFalse();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task Exists_ExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();
		FileSystem.Directory.Delete(path);

		await That(sut.Exists).IsTrue();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		await That(sut.Exists).IsFalse();
	}

	[Test]
	public async Task Exists_ForwardSlash_ShouldReturnTrue()
	{
		FileSystem.InitializeIn("/");

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("/");

		await That(sut.Exists).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task Exists_NotExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsFalse();
		FileSystem.Directory.CreateDirectory(path);

		await That(sut.Exists).IsFalse();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task Exists_ShouldNotChangeOnMoveTo(string path, string destination)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.MoveTo(destination);

		await That(sut.Exists).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task Exists_ShouldUpdateOnCreateWhenNotNetFramework(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsFalse();

		sut.Create();

		await That(sut.Exists).IsEqualTo(!Test.IsNetFramework);
	}

	[Test]
	[AutoArguments]
	public async Task Exists_ShouldUpdateOnDeleteWhenNotNetFramework(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.Delete();

		await That(sut.Exists).IsEqualTo(Test.IsNetFramework);
	}

	[Test]
	[AutoArguments]
	public async Task Exists_ShouldUpdateOnRecursiveDeleteWhenNotNetFramework(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.Delete(true);

		await That(sut.Exists).IsEqualTo(Test.IsNetFramework);
	}

	[Test]
	[AutoArguments]
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
