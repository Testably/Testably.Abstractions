#if FEATURE_FILESYSTEM_SAFEFILEHANDLE && FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

[FileSystemTests]
public class OpenHandleStreamTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task New_WithHandle_ShouldReadTheFileContent(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using FileSystemStream stream = FileSystem.FileStream.New(handle, FileAccess.Read);
		using StreamReader reader = new(stream);

		await That(reader.ReadToEnd()).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
	public async Task New_WithHandleAndBufferSize_ShouldReadTheFileContent(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using FileSystemStream stream = FileSystem.FileStream.New(handle, FileAccess.Read, 1024);
		using StreamReader reader = new(stream);

		await That(reader.ReadToEnd()).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
	public async Task New_WithHandleAndBufferSizeAndAsync_ShouldReadTheFileContent(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using FileSystemStream stream =
			FileSystem.FileStream.New(handle, FileAccess.Read, 1024, false);
		using StreamReader reader = new(stream);

		await That(reader.ReadToEnd()).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
	public async Task New_WithHandle_ShouldCreateWritableStream(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
		{
			using FileSystemStream stream =
				FileSystem.FileStream.New(handle, FileAccess.ReadWrite);
			stream.Write(new byte[] { 1, 2, 3, }, 0, 3);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 1, 2, 3, });
	}

	[Test]
	[AutoArguments]
	public async Task New_WithHandleOpenedWithCreateNew_ShouldNotThrow(string path)
	{
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);

		void Act()
		{
			using FileSystemStream stream = FileSystem.FileStream.New(handle, FileAccess.Read);
		}

		await That(Act).DoesNotThrow()
			.Because("the stream wraps the open file instead of creating it again");
	}

	[Test]
	[AutoArguments(FileMode.Create)]
	[AutoArguments(FileMode.Truncate)]
	public async Task New_WithHandleOpenedWithTruncatingMode_ShouldKeepWrittenContent(
		FileMode mode, string path)
	{
		FileSystem.File.WriteAllText(path, "foobar");
		byte[] written = [1, 2, 3,];

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			mode, FileAccess.ReadWrite, FileShare.ReadWrite))
		{
			FileSystem.RandomAccess.Write(handle, written, 0);
			using FileSystemStream stream =
				FileSystem.FileStream.New(handle, FileAccess.ReadWrite);

			await That(stream.Length).IsEqualTo(written.Length)
				.Because("the stream must not truncate the file again");
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(written);
	}

	[Test]
	[AutoArguments]
	public async Task New_WithHandleOpenedWithAppend_ShouldAllowReadWriteAccess(string path)
	{
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

		void Act()
		{
			using FileSystemStream stream =
				FileSystem.FileStream.New(handle, FileAccess.ReadWrite);
		}

		await That(Act).DoesNotThrow()
			.Because("the append mode of the handle is not re-validated against the stream access");
	}

	[Test]
	[AutoArguments]
	public async Task New_WithHandle_ShouldReportAccessFromTheGivenFileAccess(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using FileSystemStream stream = FileSystem.FileStream.New(handle, FileAccess.Read);

		await That(stream.CanRead).IsTrue();
		await That(stream.CanWrite).IsFalse();
	}
}
#endif
