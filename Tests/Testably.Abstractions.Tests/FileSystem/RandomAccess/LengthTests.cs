#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

[FileSystemTests]
public class LengthTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task GetLength_ShouldReturnTheFileLength(string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.RandomAccess.GetLength(handle)).IsEqualTo((long)contents.Length);
	}

	[Test]
	[AutoArguments]
	public async Task GetLength_OnEmptyFile_ShouldReturnZero(string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.RandomAccess.GetLength(handle)).IsEqualTo(0L);
	}

	[Test]
	[AutoArguments]
	public async Task GetLength_OnWriteOnlyHandle_ShouldStillReturnTheLength(
		string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);

		await That(FileSystem.RandomAccess.GetLength(handle)).IsEqualTo((long)contents.Length);
	}

	[Test]
	[AutoArguments]
	public async Task GetLength_AfterWriting_ShouldReflectTheNewLength(string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);

		FileSystem.RandomAccess.Write(handle, new byte[] { 1, 2, 3, }, 0);

		await That(FileSystem.RandomAccess.GetLength(handle)).IsEqualTo(3L);
	}

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	[Test]
	[AutoArguments]
	public async Task SetLength_ShouldTruncateTheFile(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4, 5,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.SetLength(handle, 2);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 1, 2, });
	}

	[Test]
	[AutoArguments]
	public async Task SetLength_ShouldGrowTheFileWithZeroes(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.SetLength(handle, 4);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 1, 2, 0, 0, });
	}

	[Test]
	[AutoArguments]
	public async Task SetLength_ToZero_ShouldEmptyTheFile(string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.SetLength(handle, 0);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEmpty();
	}

	[Test]
	[AutoArguments]
	public async Task SetLength_WithNegativeLength_ShouldThrowArgumentOutOfRangeException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);

		void Act() => FileSystem.RandomAccess.SetLength(handle, -1);

		await That(Act).Throws<ArgumentOutOfRangeException>();
	}

	[Test]
	[AutoArguments]
	public async Task SetLength_WithReadOnlyHandle_ShouldThrowUnauthorizedAccessException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		void Act() => FileSystem.RandomAccess.SetLength(handle, 1);

		await That(Act).Throws<UnauthorizedAccessException>();
	}
#endif
}
#endif
