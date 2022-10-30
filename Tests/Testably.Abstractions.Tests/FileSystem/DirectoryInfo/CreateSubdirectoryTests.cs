using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateSubdirectoryTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CreateSubdirectory_MissingParent_ShouldCreateDirectory(
		string path, string subdirectory)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeFalse();
		IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		sut.Exists.Should().BeFalse();
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
		result.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(result.FullName).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateSubdirectory_ShouldCreateDirectory(string path, string subdirectory)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Create();
		IDirectoryInfo result = sut.CreateSubdirectory(subdirectory);

		sut.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
		result.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(result.FullName).Should().BeTrue();
	}
}