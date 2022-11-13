using System.IO;
using Testably.Abstractions.FileSystem;

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
		FileSystem.File.Exists(path).Should().BeFalse();

		using FileSystemStream stream = sut.Create();

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldRefreshExistsCache_ExceptOnNetFramework(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		sut.Exists.Should().BeFalse();

		using FileSystemStream stream = sut.Create();

		if (Test.IsNetFramework)
		{
			sut.Exists.Should().BeFalse();
		}
		else
		{
			sut.Exists.Should().BeTrue();
		}

		FileSystem.File.Exists(path).Should().BeTrue();
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