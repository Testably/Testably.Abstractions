#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

[FileSystemTests]
public class AppendTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Write_OnAppendHandle_ShouldHonourTheOffset(string path)
	{
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
	public async Task Write_OnAppendHandle_AtTheLength_ShouldExtendTheFile(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Append, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, new byte[] { 9, }, 4);
		}

		await That(FileSystem.File.ReadAllBytes(path))
			.IsEqualTo(new byte[] { 1, 2, 3, 4, 9, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_OnAppendHandle_ToAnEmptyFile_ShouldWriteFromTheOffset(string path)
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
