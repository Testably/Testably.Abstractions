using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void OpenWrite_MissingFile_ShouldCreateFile(string path)
	{
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void OpenWrite_ShouldUseWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Write);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
		stream.CanRead.Should().BeFalse();
		stream.CanWrite.Should().BeTrue();
		stream.CanSeek.Should().BeTrue();
		stream.CanTimeout.Should().BeFalse();
	}
}