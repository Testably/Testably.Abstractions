#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

/// <summary>
///     Linux `pwrite(2)` appends when the descriptor carries `O_APPEND`, whatever offset is passed, contrary to
///     POSIX; Windows and macOS honour the offset.
/// </summary>
[FileSystemTests]
public class AppendTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Write_OnAppendHandle_ShouldAppend_OnLinux(string path)
	{
		Skip.IfNot(Test.RunsOnLinux, "only Linux appends regardless of the offset");

		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Append, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, new byte[] { 9, }, 0);
		}

		await That(FileSystem.File.ReadAllBytes(path))
			.IsEqualTo(new byte[] { 1, 2, 3, 4, 9, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_OnAppendHandle_ShouldHonourTheOffset_OnWindowsAndMac(string path)
	{
		Skip.If(Test.RunsOnLinux, "Linux appends regardless of the offset");

		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Append, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, new byte[] { 9, }, 0);
		}

		await That(FileSystem.File.ReadAllBytes(path))
			.IsEqualTo(new byte[] { 9, 2, 3, 4, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_OnAppendHandle_ShouldAppendToAnEmptyFileRegardlessOfPlatform(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Append, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, new byte[] { 7, 8, }, 0);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 7, 8, });
	}
}
#endif
