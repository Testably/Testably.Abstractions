#if EXECUTE_SAFEFILEHANDLE_TESTS
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Text;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

[FileSystemTests]
public partial class SafeFileHandleTests
{
	[Theory]
	[AutoData]
	public async Task New_SafeFileHandle_InvalidHandle_ShouldThrowArgumentException(
		string filename)
	{
		string path = FileSystem.Path.GetFullPath(filename);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);

		void Act()
		{
			FileSystem.FileStream.New(handle, FileAccess.ReadWrite);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName("handle");
	}

	[Theory]
	[AutoData]
	public async Task New_SafeFileHandle_InvalidHandle_WithBufferSize_ShouldThrowArgumentException(
		string filename)
	{
		string path = FileSystem.Path.GetFullPath(filename);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);

		void Act()
		{
			FileSystem.FileStream.New(handle, FileAccess.ReadWrite, 1024);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName("handle");
	}

	[Theory]
	[AutoData]
	public async Task
		New_SafeFileHandle_InvalidHandle_WithBufferSizeAndAsync_ShouldThrowArgumentException(
			string filename)
	{
		string path = FileSystem.Path.GetFullPath(filename);
		SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);

		void Act()
		{
			FileSystem.FileStream.New(handle, FileAccess.ReadWrite, 1024, true);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName("handle");
	}

	[Theory]
	[AutoData]
	public async Task New_SafeFileHandle_Valid_ShouldCreateWritableStream(
		string filename, string contents)
	{
		IDisposable? cleanup = null;
		if (FileSystem is not RealFileSystem realFileSystem)
		{
			realFileSystem = new RealFileSystem();
			cleanup = realFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
			FileSystem.InitializeIn(realFileSystem.Directory.GetCurrentDirectory());
		}

		try
		{
			string path = realFileSystem.Path.GetFullPath(filename);
			realFileSystem.File.WriteAllText(path, contents);
			FileSystem.File.WriteAllText(path, contents);
			using SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
			(FileSystem as MockFileSystem)?.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

			FileSystemStream stream = FileSystem.FileStream.New(handle, FileAccess.ReadWrite);
			stream.Write(Encoding.UTF8.GetBytes("foo"), 0, 3);
			stream.Dispose();

			FileSystem.File.ReadAllText(path).Should().StartWith("foo");
		}
		finally
		{
			cleanup?.Dispose();
		}
	}

	[Theory]
	[AutoData]
	public async Task New_SafeFileHandle_Valid_WithBufferSize_ShouldCreateWritableStream(
		string filename, string contents)
	{
		IDisposable? cleanup = null;
		if (FileSystem is not RealFileSystem realFileSystem)
		{
			realFileSystem = new RealFileSystem();
			cleanup = realFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
			FileSystem.InitializeIn(realFileSystem.Directory.GetCurrentDirectory());
		}

		try
		{
			string path = realFileSystem.Path.GetFullPath(filename);
			realFileSystem.File.WriteAllText(path, contents);
			FileSystem.File.WriteAllText(path, contents);
			using SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
			(FileSystem as MockFileSystem)?.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

			FileSystemStream stream =
				FileSystem.FileStream.New(handle, FileAccess.ReadWrite, 1024);
			stream.Write(Encoding.UTF8.GetBytes("foo"), 0, 3);
			stream.Dispose();

			FileSystem.File.ReadAllText(path).Should().StartWith("foo");
		}
		finally
		{
			cleanup?.Dispose();
		}
	}

	[Theory]
	[AutoData]
	public async Task
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

		try
		{
			string path = realFileSystem.Path.GetFullPath(filename);
			realFileSystem.File.WriteAllText(path, contents);
			FileSystem.File.WriteAllText(path, contents);
			using SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
			(FileSystem as MockFileSystem)?.WithSafeFileHandleStrategy(
				new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

			FileSystemStream stream =
				FileSystem.FileStream.New(handle, FileAccess.ReadWrite, 1024, false);
			stream.Write(Encoding.UTF8.GetBytes("foo"), 0, 3);
			stream.Dispose();

			FileSystem.File.ReadAllText(path).Should().StartWith("foo");
		}
		finally
		{
			cleanup?.Dispose();
		}
	}
}
#endif
