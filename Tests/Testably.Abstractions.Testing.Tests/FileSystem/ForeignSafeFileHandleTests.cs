#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class ForeignSafeFileHandleTests
{
	private static MockFileSystem Arrange(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));
		return fileSystem;
	}

	[Test]
	public async Task Operations_OnDisposedForeignHandle_ShouldThrowObjectDisposedException()
	{
		MockFileSystem fileSystem = Arrange("file.txt");
		SafeFileHandle handle = new(new IntPtr(0x1234), ownsHandle: false);
		handle.Dispose();

		void Act() => fileSystem.RandomAccess.GetLength(handle);

		await That(Act).Throws<ObjectDisposedException>();
	}

	[Test]
	public async Task Operations_OnNullHandle_ShouldThrowArgumentNullException()
	{
		MockFileSystem fileSystem = Arrange("file.txt");

		void Act() => fileSystem.File.GetAttributes((SafeFileHandle)null!);

		await That(Act).Throws<ArgumentNullException>();
	}

	[Test]
	public async Task Operations_WhenMappedFileIsMissing_ShouldNameThePath()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithSubdirectory("sub");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ =>
				new SafeFileHandleMock(fileSystem.Path.Combine("sub", "missing.txt"))));
		using SafeFileHandle handle = new(new IntPtr(0x1234), ownsHandle: false);

		void Act() => fileSystem.File.GetAttributes(handle);

		await That(Act).Throws<FileNotFoundException>()
			.WithMessage("*missing.txt*").AsWildcard();
	}

	[Test]
	public async Task ForeignHandle_ShouldStillResolveThroughTheStrategy()
	{
		MockFileSystem fileSystem = Arrange("file.txt");
		using SafeFileHandle handle = new(new IntPtr(0x1234), ownsHandle: false);

		await That(fileSystem.RandomAccess.GetLength(handle)).IsEqualTo(12L);
	}

	[Test]
	public async Task FileStreamFromForeignHandle_Dispose_ShouldLeaveTheHandleOpen()
	{
		MockFileSystem fileSystem = Arrange("file.txt");
		using SafeFileHandle handle = new(new IntPtr(0x1234), ownsHandle: false);

		fileSystem.FileStream.New(handle, FileAccess.Read).Dispose();

		await That(handle.IsClosed).IsFalse()
			.Because("a foreign handle may wrap an operating system handle that the caller still uses");
	}
}
#endif
