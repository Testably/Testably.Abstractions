using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Create_MissingFile_ShouldCreateFile(string path)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldUseReadWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.Create(path);

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
		stream.CanRead.Should().BeTrue();
		stream.CanWrite.Should().BeTrue();
		stream.CanSeek.Should().BeTrue();
		stream.CanTimeout.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Create_WithBufferSize_ShouldUseReadWriteAccessAndNoneShare(
		string path, int bufferSize)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.Create(path, bufferSize);

		stream.IsAsync.Should().BeFalse();
		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_WithBufferSizeAndFileOptions_ShouldUseReadWriteAccessAndNoneShare(
		string path, int bufferSize)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream =
			FileSystem.File.Create(path, bufferSize, FileOptions.Asynchronous);

		stream.IsAsync.Should().BeTrue();
		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}
}