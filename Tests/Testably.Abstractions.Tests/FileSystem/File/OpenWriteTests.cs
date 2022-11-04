using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class OpenWriteTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
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

	[SkippableTheory]
	[AutoData]
	public void OpenWrite_StreamShouldNotThrowExceptionWhenReading(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = stream.ReadByte();
		});

		exception.Should().BeNull();
	}
}