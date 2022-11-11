using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Create_ExistingFile_ShouldBeOverwritten(
		string path, string originalContent, string newContent)
	{
		FileSystem.File.WriteAllText(path, originalContent);

		using FileSystemStream stream = FileSystem.File.Create(path);

		using StreamWriter streamWriter = new(stream);
		streamWriter.Write(newContent);

		streamWriter.Dispose();
		stream.Dispose();
		FileSystem.File.ReadAllText(path).Should().Be(newContent);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ReadOnlyFile_ShouldThrowUnauthorizedAccessException(
		string path, string content)
	{
		FileSystem.File.WriteAllText(path, content);
		FileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Create(path);
		});

		exception.Should().BeOfType<UnauthorizedAccessException>()
		   .Which.HResult.Should().Be(-2147024891);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string fileName)
	{
		string filePath = FileSystem.Path.Combine(missingDirectory, fileName);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Create(filePath);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.HResult.Should().Be(-2147024893);
	}

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