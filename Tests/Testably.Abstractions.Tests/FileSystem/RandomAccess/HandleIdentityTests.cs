#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

/// <summary>
///     A handle refers to the file that was opened, not to its name.
/// </summary>
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
	public async Task DeleteOnClose_ShouldDeleteOnlyWhenTheLastHandleIsClosed(string path)
	{
		Skip.If(Test.RunsOnWindows,
			"two openers that both permit `FileShare.ReadWrite | FileShare.Delete` are refused: see #1090");

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
	public async Task DeleteOnClose_ShouldFollowTheFile_WhenItIsRenamed(
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

		await That(FileSystem.File.Exists(other)).IsFalse()
			.Because("the deletion follows the file, not the path it was opened at");
	}

	[Test]
	[AutoArguments]
	public async Task DeleteOnClose_ShouldNotDeleteAReplacementAtTheOldPath(
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

		await That(FileSystem.File.Exists(path)).IsTrue()
			.Because("the replacement at the old path is a different file");
	}
}
#endif
