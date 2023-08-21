using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Create_MissingFile_ShouldCreateFile(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		FileSystem.Should().NotHaveFile(path);

		using FileSystemStream stream = sut.Create();

		FileSystem.Should().HaveFile(path);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldRefreshExistsCacheForCurrentItem_ExceptOnNetFramework(string path)
	{
		IFileInfo sut1 = FileSystem.FileInfo.New(path);
		IFileInfo sut2 = FileSystem.FileInfo.New(path);
		IFileInfo sut3 = FileSystem.FileInfo.New(path);
		sut1.Exists.Should().BeFalse();
		sut2.Exists.Should().BeFalse();
		// Do not call Exists for `sut3`

		using FileSystemStream stream = sut1.Create();

		if (Test.IsNetFramework)
		{
			sut1.Exists.Should().BeFalse();
			sut2.Exists.Should().BeFalse();
			sut3.Exists.Should().BeTrue();
		}
		else
		{
			sut1.Exists.Should().BeTrue();
			sut2.Exists.Should().BeFalse();
			sut3.Exists.Should().BeTrue();
		}

		FileSystem.Should().HaveFile(path);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldUseReadWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.Create();

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}
}
