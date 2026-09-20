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
	public async Task DeleteOnClose_OnUnix_ShouldNotFollowTheFile_WhenItIsRenamed(
		string path, string other)
	{
		Skip.If(Test.RunsOnWindows, "Windows follows the file across a rename");

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
		Skip.If(Test.RunsOnWindows, "Windows follows the file across a rename");

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
