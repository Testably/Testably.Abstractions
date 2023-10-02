namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExistsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Exists_ArbitraryPath_ShouldBeFalse(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Should().NotExist();
		FileSystem.Should().NotHaveDirectory(sut.FullName);
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().Exist();
		FileSystem.Directory.Delete(path);

		sut.Should().Exist();
		FileSystem.Should().NotHaveDirectory(sut.FullName);
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Should().NotExist();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_NotExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().NotExist();
		FileSystem.Directory.CreateDirectory(path);

		sut.Should().NotExist();
		FileSystem.Should().HaveDirectory(sut.FullName);
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ShouldNotChangeOnMoveTo(string path, string destination)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().Exist();

		sut.MoveTo(destination);

		sut.Should().Exist();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ShouldUpdateOnCreateWhenNotNetFramework(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().NotExist();

		sut.Create();

		sut.Exists.Should().Be(!Test.IsNetFramework);
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ShouldUpdateOnDeleteWhenNotNetFramework(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().Exist();

		sut.Delete();

		sut.Exists.Should().Be(Test.IsNetFramework);
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ShouldUpdateOnRecursiveDeleteWhenNotNetFramework(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().Exist();

		sut.Delete(true);

		sut.Exists.Should().Be(Test.IsNetFramework);
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ShouldUpdateOnRefresh(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().Exist();
		FileSystem.Directory.Delete(path);
		sut.Should().Exist();

		sut.Refresh();

		sut.Should().NotExist();
	}
}
