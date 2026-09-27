#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

[FileSystemTests]
public class Tests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task RandomAccess_ShouldBeSetOnTheFileSystem()
	{
		await That(FileSystem.RandomAccess).IsNotNull();
		await That(FileSystem.RandomAccess.FileSystem).IsSameAs(FileSystem);
	}

	[Test]
	[AutoArguments]
	public async Task Operations_OnDisposedHandle_ShouldThrowObjectDisposedException(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);

		SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite);
		handle.Dispose();

		void ReadAct() => FileSystem.RandomAccess.Read(handle, new byte[1], 0);
		void WriteAct() => FileSystem.RandomAccess.Write(handle, new byte[] { 1, }, 0);
		void LengthAct() => FileSystem.RandomAccess.GetLength(handle);

		await That(ReadAct).Throws<ObjectDisposedException>();
		await That(WriteAct).Throws<ObjectDisposedException>();
		await That(LengthAct).Throws<ObjectDisposedException>();
	}

	[Test]
	[AutoArguments]
	public async Task ReadWrite_ShouldRoundTripTheContent(string path, byte[] contents)
	{
		Skip.If(contents.Length == 0);
		FileSystem.File.WriteAllBytes(path, []);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite);

		FileSystem.RandomAccess.Write(handle, contents, 0);

		byte[] buffer = new byte[contents.Length];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 0);

		await That(read).IsEqualTo(contents.Length);
		await That(buffer).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
	public async Task Write_ShouldUpdateTheFileThroughTheAbstraction(string path, byte[] contents)
	{
		Skip.If(contents.Length == 0);
		FileSystem.File.WriteAllBytes(path, []);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, contents, 0);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(contents);
	}

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	[Test]
	[AutoArguments]
	public async Task FlushToDisk_ShouldNotChangeTheContent(string path, byte[] contents)
	{
		Skip.If(contents.Length == 0);
		FileSystem.File.WriteAllBytes(path, []);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, contents, 0);
			FileSystem.RandomAccess.FlushToDisk(handle);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
	public async Task FlushToDisk_ShouldBeRepeatable(string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);

		void Act()
		{
			FileSystem.RandomAccess.FlushToDisk(handle);
			FileSystem.RandomAccess.FlushToDisk(handle);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	[AutoArguments]
	public async Task FlushToDisk_OnReadOnlyHandle_ShouldNotThrow(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read);

		void Act() => FileSystem.RandomAccess.FlushToDisk(handle);

		await That(Act).DoesNotThrow()
			.Because("the runtime ignores ERROR_ACCESS_DENIED from FlushFileBuffers on Windows so that read-only handles can be flushed on every platform");
	}

	[Test]
	[AutoArguments]
	public async Task FlushToDisk_OnDisposedHandle_ShouldThrowObjectDisposedException(string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);
		handle.Dispose();

		void Act() => FileSystem.RandomAccess.FlushToDisk(handle);

		await That(Act).Throws<ObjectDisposedException>();
	}
#endif
}
#endif
