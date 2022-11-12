#if EXECUTE_SAFEFILEHANDLE_TESTS
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Text;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class SafeFileHandleTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void New_SafeFileHandle_Invalid_ShouldThrowArgumentException(
		string filename, string contents)
	{
		string path = FileSystem.Path.GetFullPath(filename);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(handle, FileAccess.ReadWrite);
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("handle");
	}

	[SkippableTheory]
	[AutoData]
	public void New_SafeFileHandle_Invalid_WithBufferSize_ShouldThrowArgumentException(
		string filename, string contents)
	{
		string path = FileSystem.Path.GetFullPath(filename);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(handle, FileAccess.ReadWrite, 1024);
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("handle");
	}

	[SkippableTheory]
	[AutoData]
	public void
		New_SafeFileHandle_Invalid_WithBufferSizeAndAsync_ShouldThrowArgumentException(
			string filename, string contents)
	{
		string path = FileSystem.Path.GetFullPath(filename);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(handle, FileAccess.ReadWrite, 1024, true);
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("handle");
	}

	[SkippableTheory]
	[AutoData]
	public void New_SafeFileHandle_Valid_ShouldCreateWritableStream(
		string filename, string contents)
	{
		IDisposable? cleanup = null;
		if (FileSystem is not RealFileSystem realFileSystem)
		{
			realFileSystem = new RealFileSystem();
			cleanup = realFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
			FileSystem.InitializeIn(realFileSystem.Directory.GetCurrentDirectory());
		}
		string path = realFileSystem.Path.GetFullPath(filename);
		realFileSystem.File.WriteAllText(path, contents);
		FileSystem.File.WriteAllText(path, contents);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
		(FileSystem as MockFileSystem)?.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		FileSystemStream stream = FileSystem.FileStream.New(handle, FileAccess.ReadWrite);
		stream.Write(Encoding.UTF8.GetBytes("foo"), 0, 3);
		stream.Dispose();

		FileSystem.File.ReadAllText(path).Should().StartWith("foo");
		cleanup?.Dispose();
	}

	[SkippableTheory]
	[AutoData]
	public void New_SafeFileHandle_Valid_WithBufferSize_ShouldCreateWritableStream(
		string filename, string contents)
	{
		IDisposable? cleanup = null;
		if (FileSystem is not RealFileSystem realFileSystem)
		{
			realFileSystem = new RealFileSystem();
			cleanup = realFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
			FileSystem.InitializeIn(realFileSystem.Directory.GetCurrentDirectory());
		}
		string path = realFileSystem.Path.GetFullPath(filename);
		realFileSystem.File.WriteAllText(path, contents);
		FileSystem.File.WriteAllText(path, contents);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
		(FileSystem as MockFileSystem)?.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		FileSystemStream stream =
			FileSystem.FileStream.New(handle, FileAccess.ReadWrite, 1024);
		stream.Write(Encoding.UTF8.GetBytes("foo"), 0, 3);
		stream.Dispose();

		FileSystem.File.ReadAllText(path).Should().StartWith("foo");
		cleanup?.Dispose();
	}

	[SkippableTheory]
	[AutoData]
	public void
		New_SafeFileHandle_Valid_WithBufferSizeAndAsync_ShouldCreateWritableStream(
			string filename, string contents)
	{
		IDisposable? cleanup = null;
		if (FileSystem is not RealFileSystem realFileSystem)
		{
			realFileSystem = new RealFileSystem();
			cleanup = realFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
			FileSystem.InitializeIn(realFileSystem.Directory.GetCurrentDirectory());
		}
		string path = realFileSystem.Path.GetFullPath(filename);
		realFileSystem.File.WriteAllText(path, contents);
		FileSystem.File.WriteAllText(path, contents);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
		(FileSystem as MockFileSystem)?.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		FileSystemStream stream =
			FileSystem.FileStream.New(handle, FileAccess.ReadWrite, 1024, false);
		stream.Write(Encoding.UTF8.GetBytes("foo"), 0, 3);
		stream.Dispose();

		FileSystem.File.ReadAllText(path).Should().StartWith("foo");
		cleanup?.Dispose();
	}
}
#endif