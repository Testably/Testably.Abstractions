#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

[FileSystemTests]
public class HandleIdentityTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Handle_ShouldKeepWorking_WhenTheFileIsRenamed(string path, string other)
	{
		Skip.If(Test.RunsOnWindows,
			"moving a file that a handle holds open needs `ignoreFileShare`, which is inert on Windows: see #1086");

		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

		FileSystem.File.Move(path, other);

		byte[] buffer = new byte[4];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 0);

		await That(read).IsEqualTo(4);
		await That(buffer).IsEqualTo(new byte[] { 1, 2, 3, 4, });
	}

	[Test]
	[AutoArguments]
	public async Task Handle_ShouldSeeTheNewContent_WhenTheFileIsOverwrittenByCopy(
		string path, string source)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);
		FileSystem.File.WriteAllBytes(source, [9, 9,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

		FileSystem.File.Copy(source, path, overwrite: true);

		byte[] buffer = new byte[4];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 0);

		await That(buffer.AsSpan(0, read).ToArray()).IsEqualTo(new byte[] { 9, 9, })
			.Because("copying over an existing file overwrites that file instead of replacing it");
	}

	[Test]
	[AutoArguments]
	public async Task Handle_ShouldNotFollowTheName_WhenAnotherFileTakesTheOldPath(
		string path, string other)
	{
		Skip.If(Test.RunsOnWindows,
			"moving a file that a handle holds open needs `ignoreFileShare`, which is inert on Windows: see #1086");

		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

		FileSystem.File.Move(path, other);
		FileSystem.File.WriteAllBytes(path, [9, 9, 9, 9,]);

		byte[] buffer = new byte[4];
		FileSystem.RandomAccess.Read(handle, buffer, 0);

		await That(buffer).IsEqualTo(new byte[] { 1, 2, 3, 4, })
			.Because("the handle refers to the file it was opened on, not to the path");
	}

	[Test]
	[AutoArguments]
	public async Task Handle_ShouldKeepTheContent_WhenTheFileIsDeleted(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

		FileSystem.File.Delete(path);

		byte[] buffer = new byte[4];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 0);

		await That(FileSystem.RandomAccess.GetLength(handle)).IsEqualTo(4);
		await That(read).IsEqualTo(4);
		await That(buffer).IsEqualTo(new byte[] { 1, 2, 3, 4, })
			.Because("deleting removes the name, not the file an open handle refers to");
	}

	[Test]
	[AutoArguments]
	public async Task Handle_ShouldKeepWorking_WhenTheFileIsDeletedAndWrittenTo(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

		FileSystem.File.Delete(path);
		FileSystem.RandomAccess.Write(handle, new byte[] { 5, 6, }, 3);

		byte[] buffer = new byte[5];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 0);

		await That(read).IsEqualTo(5);
		await That(buffer).IsEqualTo(new byte[] { 1, 2, 3, 5, 6, });
		await That(FileSystem.File.Exists(path)).IsFalse()
			.Because("writing through the handle does not bring the name back");
	}

	[Test]
	[AutoArguments]
	public async Task Handle_ShouldNotAffectANewFile_WhenTheFileIsDeletedAndRecreated(
		string path)
	{
		Skip.If(Test.RunsOnWindows,
			"the mock keeps the share lock of a deleted file on its path, so it refuses to recreate the file while a handle is open");

		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

		FileSystem.File.Delete(path);
		FileSystem.File.WriteAllBytes(path, [9, 9,]);
		FileSystem.RandomAccess.Write(handle, new byte[] { 5, }, 0);

		byte[] buffer = new byte[4];
		FileSystem.RandomAccess.Read(handle, buffer, 0);

		await That(buffer).IsEqualTo(new byte[] { 5, 2, 3, 4, });
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 9, 9, })
			.Because("the handle refers to the deleted file, not to the new one at its path");
	}

	[Test]
	[AutoArguments]
	public async Task DeleteOnClose_OnUnix_ShouldDeleteAsSoonAsThatHandleCloses(string path)
	{
		Skip.If(Test.RunsOnWindows, "Windows deletes once the last handle closes");

		FileSystem.File.WriteAllText(path, null);

		SafeFileHandle first = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete,
			FileOptions.DeleteOnClose);
		using SafeFileHandle second = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

		first.Dispose();

		await That(FileSystem.File.Exists(path)).IsFalse()
			.Because("unlinking the name does not wait for other handles");
	}

	[Test]
	[AutoArguments]
	public async Task DeleteOnClose_OnWindows_ShouldDeleteOnlyWhenTheLastHandleIsClosed(string path)
	{
		Skip.IfNot(Test.RunsOnWindows, "Unix unlinks the name as soon as that handle closes");

		FileSystem.File.WriteAllText(path, null);

		SafeFileHandle first = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete,
			FileOptions.DeleteOnClose);
		SafeFileHandle second = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

		first.Dispose();

		await That(FileSystem.File.Exists(path)).IsTrue()
			.Because("a second handle is still open on the file");

		second.Dispose();

		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task DeleteOnClose_OnUnix_ShouldNotFollowTheFile_WhenItIsRenamed(
		string path, string other)
	{
		Skip.If(Test.RunsOnWindows,
			"moving a file that a handle holds open needs `ignoreFileShare`, which is inert on Windows: see #1086");

		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete,
			FileOptions.DeleteOnClose))
		{
			FileSystem.File.Move(path, other);
		}

		await That(FileSystem.File.Exists(other)).IsTrue()
			.Because("the name that was opened is unlinked, not the file");
	}

	[Test]
	[AutoArguments]
	public async Task DeleteOnClose_OnUnix_ShouldDeleteAReplacementAtTheOldPath(
		string path, string other)
	{
		Skip.If(Test.RunsOnWindows,
			"moving a file that a handle holds open needs `ignoreFileShare`, which is inert on Windows: see #1086");

		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete,
			FileOptions.DeleteOnClose))
		{
			FileSystem.File.Move(path, other);
			FileSystem.File.WriteAllText(path, "a different file");
		}

		await That(FileSystem.File.Exists(path)).IsFalse()
			.Because("the name is unlinked whatever now sits under it");
	}
}
#endif
