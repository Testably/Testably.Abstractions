#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.CompilerServices;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class MockSafeFileHandleTests
{
	[Test]
	public async Task DeleteOnClose_OnUnix_WhenADirectoryTookTheName_ShouldNotThrowFromAnUnrelatedCall()
	{
		MockFileSystem fileSystem = new(o => o.SimulatingOperatingSystem(SimulationMode.Linux));

		SafeFileHandle handle = fileSystem.File.OpenHandle("/y", FileMode.Create,
			FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, FileOptions.DeleteOnClose);
		fileSystem.File.Delete("/y");
		fileSystem.Directory.CreateDirectory("/y");
		handle.Dispose();

		void Act() => fileSystem.File.Exists("/unrelated.txt");

		await That(Act).DoesNotThrow()
			.Because("a failed unlink at close is ignored, as it is on a real file system");
		await That(fileSystem.Directory.Exists("/y")).IsTrue();
	}

	[Test]
	public async Task DeleteOnClose_OnUnix_WhenTheParentIsGone_ShouldNotThrowFromAnUnrelatedCall()
	{
		MockFileSystem fileSystem = new(o => o.SimulatingOperatingSystem(SimulationMode.Linux));
		fileSystem.Directory.CreateDirectory("/sub");

		SafeFileHandle handle = fileSystem.File.OpenHandle("/sub/x", FileMode.Create,
			FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, FileOptions.DeleteOnClose);
		fileSystem.File.Delete("/sub/x");
		fileSystem.Directory.Delete("/sub");
		handle.Dispose();

		void Act() => fileSystem.File.Exists("/unrelated.txt");

		await That(Act).DoesNotThrow()
			.Because("a failed unlink at close is ignored, as it is on a real file system");
	}

	[Test]
	public async Task DeleteOnClose_OnWindows_WhenAStreamStillHoldsTheFile_ShouldDeleteOnTheLastClose()
	{
		MockFileSystem fileSystem = new(o => o.SimulatingOperatingSystem(SimulationMode.Windows));
		fileSystem.File.WriteAllText("f.txt", "x");

		SafeFileHandle handle = fileSystem.File.OpenHandle("f.txt",
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.DeleteOnClose);
		FileSystemStream stream = fileSystem.File.Open("f.txt",
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		handle.Dispose();

		await That(fileSystem.File.Exists("f.txt")).IsTrue()
			.Because("a stream still holds the file");

		stream.Dispose();

		await That(fileSystem.File.Exists("f.txt")).IsFalse()
			.Because("the file is removed once the last holder releases it");
	}

	[Test]
	public async Task
		DeleteOnClose_OnWindows_WhenAStreamStillHoldsTheFile_ShouldNotThrowFromAnUnrelatedCall()
	{
		MockFileSystem fileSystem = new(o => o.SimulatingOperatingSystem(SimulationMode.Windows));
		fileSystem.File.WriteAllText("f.txt", "x");

		SafeFileHandle handle = fileSystem.File.OpenHandle("f.txt",
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.DeleteOnClose);
		using FileSystemStream stream = fileSystem.File.Open("f.txt",
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		handle.Dispose();

		void Act() => fileSystem.File.Exists("unrelated.txt");

		await That(Act).DoesNotThrow()
			.Because("a pending deletion must not surface from an unrelated call");
	}

	[Test]
	public async Task FileStreamNew_WithNullHandle_ShouldThrowArgumentNullException()
	{
		MockFileSystem fileSystem = new();

		void Act() => fileSystem.FileStream.New((SafeFileHandle)null!, FileAccess.Read);

		await That(Act).Throws<ArgumentNullException>();
	}

	[Test]
	public async Task OpenHandle_WhenNeverDisposed_ShouldReleaseTheShareLockOnceCollected()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("f.txt", "x");

		OpenAndDrop(fileSystem, "f.txt");
		GC.Collect();
		GC.WaitForPendingFinalizers();

		void Act()
		{
			using SafeFileHandle handle = fileSystem.File.OpenHandle("f.txt",
				FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		}

		await That(Act).DoesNotThrow()
			.Because("a handle that is no longer referenced is closed, as a real one would be when finalized");
	}

	[Test]
	public async Task Write_AtAnOffsetThatOverflows_ShouldThrowIOException()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllBytes("f.txt", [1,]);

		using SafeFileHandle handle = fileSystem.File.OpenHandle("f.txt",
			FileMode.Open, FileAccess.Write);

		void Act() => fileSystem.RandomAccess.Write(handle, new byte[] { 9, }, long.MaxValue);

		await That(Act).Throws<IOException>();
	}

	[Test]
	public async Task Write_WhenAChangingInterceptionThrows_ShouldLeaveTheContentUnchanged()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllBytes("f.txt", [1, 2, 3,]);

		using (fileSystem.Intercept.Changing(FileSystemTypes.File,
			       _ => throw new InvalidOperationException("vetoed")))
		{
			using SafeFileHandle handle = fileSystem.File.OpenHandle("f.txt",
				FileMode.Open, FileAccess.Write, FileShare.ReadWrite);

			void Act() => fileSystem.RandomAccess.Write(handle, new byte[] { 9, }, 0);

			await That(Act).Throws<InvalidOperationException>();
		}

		await That(fileSystem.File.ReadAllBytes("f.txt"))
			.IsEqualTo(new byte[] { 1, 2, 3, })
			.Because("the write was vetoed before it was published");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void OpenAndDrop(MockFileSystem fileSystem, string path)
		=> _ = fileSystem.File.OpenHandle(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
}
#endif
