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
	public async Task New_WithHandle_Dispose_ShouldCloseTheHandle(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		FileSystem.FileStream.New(handle, FileAccess.ReadWrite).Dispose();

		await That(handle.IsClosed).IsTrue()
			.Because("a stream created from a handle owns it");
	}

	[Test]
	[AutoArguments]
	public async Task New_WithHandle_Dispose_ShouldReleaseTheFileShareOfTheHandle(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		FileSystem.FileStream.New(handle, FileAccess.ReadWrite).Dispose();

		using SafeFileHandle second = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.None);

		await That(second.IsInvalid).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task New_WithHandleOpenedWithDeleteOnClose_Dispose_ShouldDeleteTheFile(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,
			FileOptions.DeleteOnClose);
		FileSystem.FileStream.New(handle, FileAccess.ReadWrite).Dispose();

		await That(FileSystem.File.Exists(path)).IsFalse()
			.Because("disposing the stream closes the handle, which deletes the file");
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
