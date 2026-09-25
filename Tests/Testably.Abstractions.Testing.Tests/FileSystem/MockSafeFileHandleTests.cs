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
	[Arguments(FileMode.Create)]
	[Arguments(FileMode.Truncate)]
	public async Task OpenHandle_WhenTruncationThrows_ShouldReleaseTheShareLock(FileMode mode)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("f.txt", "x");

		using (fileSystem.Intercept.Changing(FileSystemTypes.File,
			       _ => throw new InvalidOperationException("vetoed")))
		{
			void OpenVetoed() => fileSystem.File.OpenHandle("f.txt",
				mode, FileAccess.Write, FileShare.None);

			await That(OpenVetoed).Throws<InvalidOperationException>();
		}

		void Act()
		{
			using SafeFileHandle handle = fileSystem.File.OpenHandle("f.txt",
				FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		}

		await That(Act).DoesNotThrow()
			.Because("an open that failed must not keep holding the file");
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
	public async Task OpenHandle_WhenUsedAndNeverDisposed_ShouldReleaseTheShareLockOnceCollected()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("f.txt", "x");

		OpenUseAndDrop(fileSystem, "f.txt");
		GC.Collect();
		GC.WaitForPendingFinalizers();

		void Act()
		{
			using SafeFileHandle handle = fileSystem.File.OpenHandle("f.txt",
				FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		}

		await That(Act).DoesNotThrow()
			.Because("recording the handle in the statistics must not keep it alive");
	}

	[Test]
	public async Task RandomAccess_WithHandleFromAnotherFileSystem_ShouldNotBeReportedAsClosed()
	{
		MockFileSystem fileSystemA = new();
		MockFileSystem fileSystemB = new();
		fileSystemA.File.WriteAllText("a.txt", "a");
		fileSystemB.File.WriteAllText("b.txt", "b");
		fileSystemB.File.OpenHandle("b.txt").Dispose();
		fileSystemB.File.WriteAllText("c.txt", "cc");
		fileSystemB.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("c.txt")));

		using SafeFileHandle handleA = fileSystemA.File.OpenHandle("a.txt");

		void Act() => fileSystemB.RandomAccess.GetLength(handleA);

		await That(Act).DoesNotThrow()
			.Because("a live handle from another file system was never closed");
	}

	[Test]
	public async Task RandomAccess_WithHandleFromAnotherFileSystem_ShouldUseTheSafeFileHandleStrategy()
	{
		MockFileSystem fileSystemA = new();
		MockFileSystem fileSystemB = new();
		fileSystemA.File.WriteAllText("a.txt", "a");
		fileSystemB.File.WriteAllText("b.txt", "b");
		fileSystemB.File.WriteAllText("c.txt", "cc");
		fileSystemB.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock("c.txt")));

		using SafeFileHandle handleA = fileSystemA.File.OpenHandle("a.txt");
		using SafeFileHandle handleB = fileSystemB.File.OpenHandle("b.txt");

		long result = fileSystemB.RandomAccess.GetLength(handleA);

		await That(result).IsEqualTo(2)
			.Because("a handle from another file system is foreign and must be mapped by the strategy");
	}

	[Test]
	public async Task Statistics_WhenHandleWasCollected_ShouldStillDescribeTheCall()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("f.txt", "x");

		OpenUseAndDrop(fileSystem, "f.txt");
		GC.Collect();
		GC.WaitForPendingFinalizers();

		await That(fileSystem.Statistics.RandomAccess.Methods[0].ToString())
			.IsEqualTo($"GetLength({typeof(SafeFileHandle)})")
			.Because("the description of a handle is kept, even when the handle itself is not");
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

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void OpenUseAndDrop(MockFileSystem fileSystem, string path)
		=> _ = fileSystem.RandomAccess.GetLength(
			fileSystem.File.OpenHandle(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
}
#endif
