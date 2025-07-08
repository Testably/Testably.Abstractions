using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class CreateTests
{
	[Theory]
	[AutoData]
	public void Create_ExistingFile_ShouldBeOverwritten(
		string path, string originalContent, string newContent)
	{
		FileSystem.File.WriteAllText(path, originalContent);

		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			using StreamWriter streamWriter = new(stream);
			streamWriter.Write(newContent);
		}

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(newContent);
	}

	[Theory]
	[AutoData]
	public void Create_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string fileName)
	{
		string filePath = FileSystem.Path.Combine(missingDirectory, fileName);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Create(filePath);
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public void Create_MissingFile_ShouldCreateFile(string path)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[Theory]
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

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}

	[Theory]
	[AutoData]
	public async Task Create_ShouldUseReadWriteAccessAndNoneShare(string path)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
		await That(stream.CanRead).IsTrue();
		await That(stream.CanWrite).IsTrue();
		await That(stream.CanSeek).IsTrue();
		await That(stream.CanTimeout).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Create_WithBufferSize_ShouldUseReadWriteAccessAndNoneShare(
		string path, int bufferSize)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.Create(path, bufferSize);

		await That(stream.IsAsync).IsFalse();
		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}

	[Theory]
	[AutoData]
	public async Task Create_WithBufferSizeAndFileOptions_ShouldUseReadWriteAccessAndNoneShare(
		string path, int bufferSize)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream =
			FileSystem.File.Create(path, bufferSize, FileOptions.Asynchronous);

		await That(stream.IsAsync).IsTrue();
		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}
}
