using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class OpenReadTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void OpenRead_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = sut.OpenRead();
		});

		exception.Should().BeException<FileNotFoundException>($"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public void OpenRead_ShouldUseReadAccessAndReadShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.OpenRead();

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Read);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(
			Test.RunsOnWindows ? FileShare.Read : FileShare.ReadWrite);
	}
}