using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class OpenWriteTests
{
	[Theory]
	[AutoData]
	public void OpenWrite_MissingFile_ShouldCreateFile(string path)
	{
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public async Task OpenWrite_ShouldOverwriteExistingFile(string path, string previousContent)
	{
		FileSystem.File.WriteAllText(path, previousContent);

		using FileSystemStream stream = FileSystem.File.OpenWrite(path);
		using (StreamWriter streamWriter = new(stream))
		{
			streamWriter.Write("new-content");
		}

		string result = FileSystem.File.ReadAllText(path);

		await That(result).StartsWith("new-content");
		await That(result.Length).IsEqualTo(previousContent.Length);
	}

	[Theory]
	[AutoData]
	public async Task OpenWrite_ShouldUseWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Write);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
		await That(stream.CanRead).IsFalse();
		await That(stream.CanWrite).IsTrue();
		await That(stream.CanSeek).IsTrue();
		await That(stream.CanTimeout).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task OpenWrite_StreamShouldNotThrowExceptionWhenReading(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			_ = stream.ReadByte();
		});

		await That(exception).IsNull();
	}
}
