using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class OpenReadTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void OpenRead_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.File.OpenRead(path);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void OpenRead_ShouldUseReadAccessAndReadShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Read);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(
			Test.RunsOnWindows ? FileShare.Read : FileShare.ReadWrite);
		stream.CanRead.Should().BeTrue();
		stream.CanWrite.Should().BeFalse();
		stream.CanSeek.Should().BeTrue();
		stream.CanTimeout.Should().BeFalse();
	}
}