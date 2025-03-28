namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[AutoData]
	public void Exists_ArbitraryPath_ShouldBeFalse(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Exists.Should().BeFalse();
		FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Exists_ExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();
		FileSystem.Directory.Delete(path);

		sut.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Exists_File_ShouldReturnFalse(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Exists.Should().BeFalse();
	}

	[Fact]
	public void Exists_ForwardSlash_ShouldReturnTrue()
	{
		FileSystem.InitializeIn("D:");

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("/");

		sut.Exists.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Exists_NotExistedPreviously_ShouldOnlyUpdateOnInitialization(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeFalse();
		FileSystem.Directory.CreateDirectory(path);

		sut.Exists.Should().BeFalse();
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Exists_ShouldNotChangeOnMoveTo(string path, string destination)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();

		sut.MoveTo(destination);

		sut.Exists.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Exists_ShouldUpdateOnCreateWhenNotNetFramework(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeFalse();

		sut.Create();

		sut.Exists.Should().Be(!Test.IsNetFramework);
	}

	[Theory]
	[AutoData]
	public void Exists_ShouldUpdateOnDeleteWhenNotNetFramework(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();

		sut.Delete();

		sut.Exists.Should().Be(Test.IsNetFramework);
	}

	[Theory]
	[AutoData]
	public void Exists_ShouldUpdateOnRecursiveDeleteWhenNotNetFramework(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();

		sut.Delete(true);

		sut.Exists.Should().Be(Test.IsNetFramework);
	}

	[Theory]
	[AutoData]
	public void Exists_ShouldUpdateOnRefresh(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();
		FileSystem.Directory.Delete(path);
		sut.Exists.Should().BeTrue();

		sut.Refresh();

		sut.Exists.Should().BeFalse();
	}
}
