#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class OpenHandleTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task OpenHandle_ShouldReturnOpenHandle(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(handle.IsInvalid).IsFalse();
		await That(handle.IsClosed).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_ShouldDefaultToOpeningExistingFileForReading(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.RandomAccess.GetLength(handle))
			.IsEqualTo(FileSystem.File.ReadAllBytes(path).Length);
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WhenFileIsMissing_ShouldThrowFileNotFoundException(string path)
	{
		void Act()
		{
			using SafeFileHandle handle = FileSystem.File.OpenHandle(path);
		}

		await That(Act).Throws<FileNotFoundException>();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WithCreateNew_WhenFileExists_ShouldThrowIOException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		void Act()
		{
			using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
				FileMode.CreateNew, FileAccess.Write);
		}

		await That(Act).Throws<IOException>();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WithCreate_ShouldCreateMissingFile(string path)
	{
		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Create, FileAccess.Write))
		{
			await That(handle.IsInvalid).IsFalse();
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WithCreate_WhenFileExists_ShouldTruncateContent(
		string path, string contents)
	{
		Skip.If(string.IsNullOrEmpty(contents));
		FileSystem.File.WriteAllText(path, contents);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Create, FileAccess.Write))
		{
			await That(FileSystem.RandomAccess.GetLength(handle)).IsEqualTo(0L);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEmpty();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WithTruncate_ShouldEmptyExistingFile(
		string path, string contents)
	{
		Skip.If(string.IsNullOrEmpty(contents));
		FileSystem.File.WriteAllText(path, contents);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Truncate, FileAccess.Write))
		{
			await That(FileSystem.RandomAccess.GetLength(handle)).IsEqualTo(0L);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEmpty();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WithTruncate_WhenFileIsMissing_ShouldThrowFileNotFoundException(
		string path)
	{
		void Act()
		{
			using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
				FileMode.Truncate, FileAccess.Write);
		}

		await That(Act).Throws<FileNotFoundException>();
	}

	[Test]
	[AutoArguments(FileMode.Truncate)]
	[AutoArguments(FileMode.CreateNew)]
	[AutoArguments(FileMode.Create)]
	[AutoArguments(FileMode.Append)]
	public async Task OpenHandle_WithReadAccess_WhenModeRequiresWriting_ShouldThrowArgumentException(
		FileMode mode, string path)
	{
		void Act()
		{
			using SafeFileHandle handle =
				FileSystem.File.OpenHandle(path, mode, FileAccess.Read);
		}

		await That(Act).Throws<ArgumentException>();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WithNegativePreallocationSize_ShouldThrowArgumentOutOfRangeException(
		string path)
	{
		void Act()
		{
			using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
				FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.None, -1);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WhenDisposed_ShouldBeClosed(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		SafeFileHandle handle = FileSystem.File.OpenHandle(path);
		handle.Dispose();

		await That(handle.IsClosed).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WhenDisposed_ShouldNotBeUsableAnyMore(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		SafeFileHandle handle = FileSystem.File.OpenHandle(path);
		handle.Dispose();

		void Act() => FileSystem.RandomAccess.GetLength(handle);

		await That(Act).Throws<ObjectDisposedException>();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WithDeleteOnClose_ShouldDeleteFileWhenClosed(string path)
	{
		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.DeleteOnClose))
		{
			await That(FileSystem.File.Exists(path)).IsTrue();
		}

		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_TwoHandlesOnSameFile_ShouldShareContent(
		string path, byte[] bytes)
	{
		Skip.If(bytes.Length == 0);
		FileSystem.File.WriteAllBytes(path, []);

		using SafeFileHandle writer = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
		using SafeFileHandle reader = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

		FileSystem.RandomAccess.Write(writer, bytes, 0);

		byte[] buffer = new byte[bytes.Length];
		int read = FileSystem.RandomAccess.Read(reader, buffer, 0);

		await That(read).IsEqualTo(bytes.Length);
		await That(buffer).IsEqualTo(bytes);
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_WithFileShareNone_ShouldBlockSecondHandle(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle first = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.None);

		void Act()
		{
			using SafeFileHandle second = FileSystem.File.OpenHandle(path,
				FileMode.Open, FileAccess.Read, FileShare.None);
		}

		await That(Act).Throws<IOException>();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_AfterClose_ShouldReleaseFileShareLock(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		SafeFileHandle first = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.None);
		first.Dispose();

		using SafeFileHandle second = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.None);

		await That(second.IsInvalid).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task OpenHandle_ShouldBeUsableByFileMethodsTakingAHandle(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetAttributes(handle))
			.IsEqualTo(FileSystem.File.GetAttributes(path));
	}
}
#endif
