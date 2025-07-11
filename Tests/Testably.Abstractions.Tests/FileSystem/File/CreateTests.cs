using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class CreateTests
{
	[Theory]
	[AutoData]
	public async Task Create_ExistingFile_ShouldBeOverwritten(
		string path, string originalContent, string newContent)
	{
		FileSystem.File.WriteAllText(path, originalContent);

		using (FileSystemStream stream = FileSystem.File.Create(path))
		{
			using StreamWriter streamWriter = new(stream);
			streamWriter.Write(newContent);
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(newContent);
	}

	[Theory]
	[AutoData]
	public async Task Create_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string fileName)
	{
		string filePath = FileSystem.Path.Combine(missingDirectory, fileName);

		void Act()
		{
			FileSystem.File.Create(filePath);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Create_MissingFile_ShouldCreateFile(string path)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Create_ReadOnlyFile_ShouldThrowUnauthorizedAccessException(
		string path, string content)
	{
		FileSystem.File.WriteAllText(path, content);
		FileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

		void Act()
		{
			FileSystem.File.Create(path);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Theory]
	[AutoData]
	public async Task Create_ShouldUseReadWriteAccessAndNoneShare(string path)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		await That(FileTestHelper.CheckFileAccess(stream)).IsEqualTo(FileAccess.ReadWrite);
		await That(FileTestHelper.CheckFileShare(FileSystem, path)).IsEqualTo(FileShare.None);
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
		await That(FileTestHelper.CheckFileAccess(stream)).IsEqualTo(FileAccess.ReadWrite);
		await That(FileTestHelper.CheckFileShare(FileSystem, path)).IsEqualTo(FileShare.None);
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
		await That(FileTestHelper.CheckFileAccess(stream)).IsEqualTo(FileAccess.ReadWrite);
		await That(FileTestHelper.CheckFileShare(FileSystem, path)).IsEqualTo(FileShare.None);
	}
}
