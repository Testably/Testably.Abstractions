#if CAN_SIMULATE_OTHER_OS
using Microsoft.Win32.SafeHandles;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

/// <summary>
///     A real <see cref="FileStream" /> adopts the <see cref="SafeFileHandle" /> it is given: the file is not opened
///     again, no new file share is taken, and layering a stream on a handle therefore cannot conflict with whatever
///     already holds the file open.
/// </summary>
public class FileStreamFactoryMockAdoptHandleTests
{
	private static MockFileSystem ArrangeWithHandle(out SafeFileHandle handle, string path)
	{
		MockFileSystem fileSystem = new(o => o.SimulatingOperatingSystem(SimulationMode.Windows));
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));
		handle = new SafeFileHandle(new IntPtr(0x1234), ownsHandle: false);
		return fileSystem;
	}

	[Test]
	public async Task New_WithHandle_ShouldNotTakeANewFileShare()
	{
		const string path = "file.txt";
		MockFileSystem fileSystem = ArrangeWithHandle(out SafeFileHandle handle, path);

		using FileSystemStream exclusive = fileSystem.File.Open(path,
			FileMode.Open, FileAccess.Read, FileShare.None);

		void Act()
		{
			using FileSystemStream adopted =
				fileSystem.FileStream.New(handle, FileAccess.Read);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task New_WithHandleAndBufferSize_ShouldNotTakeANewFileShare()
	{
		const string path = "file.txt";
		MockFileSystem fileSystem = ArrangeWithHandle(out SafeFileHandle handle, path);

		using FileSystemStream exclusive = fileSystem.File.Open(path,
			FileMode.Open, FileAccess.Read, FileShare.None);

		void Act()
		{
			using FileSystemStream adopted =
				fileSystem.FileStream.New(handle, FileAccess.Read, 1024);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task New_WithHandleAndIsAsync_ShouldNotTakeANewFileShare()
	{
		const string path = "file.txt";
		MockFileSystem fileSystem = ArrangeWithHandle(out SafeFileHandle handle, path);

		using FileSystemStream exclusive = fileSystem.File.Open(path,
			FileMode.Open, FileAccess.Read, FileShare.None);

		void Act()
		{
			using FileSystemStream adopted =
				fileSystem.FileStream.New(handle, FileAccess.Read, 1024, true);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task New_WithHandleAndIsAsync_ShouldCreateAnAsynchronousStream()
	{
		const string path = "file.txt";
		MockFileSystem fileSystem = ArrangeWithHandle(out SafeFileHandle handle, path);

		using FileSystemStream adopted =
			fileSystem.FileStream.New(handle, FileAccess.Read, 1024, true);

		await That(adopted.IsAsync).IsTrue();
	}

	[Test]
	public async Task New_WithHandle_ShouldReadTheFileContent()
	{
		const string path = "file.txt";
		MockFileSystem fileSystem = ArrangeWithHandle(out SafeFileHandle handle, path);

		using FileSystemStream adopted = fileSystem.FileStream.New(handle, FileAccess.Read);
		using StreamReader reader = new(adopted);

		await That(reader.ReadToEnd()).IsEqualTo("some content");
	}
}
#endif
