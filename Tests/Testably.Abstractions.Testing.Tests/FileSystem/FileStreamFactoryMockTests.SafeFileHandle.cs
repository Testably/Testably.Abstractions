#if EXECUTE_SAFEFILEHANDLE_TESTS
using Microsoft.Win32.SafeHandles;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public sealed partial class FileStreamFactoryMockTests
{
	[SkippableTheory]
	[AutoData]
	public void MissingFile_ShouldThrowFileNotFoundException(
		string path, string contents)
	{
		Skip.If(Test.IsNetFramework);

		path = RealFileSystem.Path.GetFullPath(path);
		RealFileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
		handle.IsInvalid.Should().BeFalse();
		MockFileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			MockFileSystem.FileStream.New(handle, FileAccess.Read);
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should().Contain($"'{MockFileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void UnregisteredFileHandle_ShouldThrowNotSupportedException(
		string path, string contents)
	{
		path = RealFileSystem.Path.GetFullPath(path);
		RealFileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
		handle.IsInvalid.Should().BeFalse();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			MockFileSystem.FileStream.New(handle, FileAccess.Read);
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should().Contain(nameof(MockFileSystem) + "." +
			                                nameof(MockFileSystem.WithSafeFileHandleStrategy));
	}

	[SkippableTheory]
	[AutoData]
	public void UnregisteredFileHandle_WithBufferSize_ShouldThrowNotSupportedException(
		string path, string contents, int bufferSize)
	{
		path = RealFileSystem.Path.GetFullPath(path);
		RealFileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
		handle.IsInvalid.Should().BeFalse();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			MockFileSystem.FileStream.New(handle, FileAccess.Read, bufferSize);
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should().Contain(nameof(MockFileSystem) + "." +
			                                nameof(MockFileSystem.WithSafeFileHandleStrategy));
	}

	[SkippableTheory]
	[AutoData]
	public void
		UnregisteredFileHandle_WithBufferSizeAndIsAsync_ShouldThrowNotSupportedException(
			string path, string contents, int bufferSize, bool isAsync)
	{
		path = RealFileSystem.Path.GetFullPath(path);
		RealFileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = UnmanagedFileLoader.CreateSafeFileHandle(path);
		handle.IsInvalid.Should().BeFalse();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			MockFileSystem.FileStream.New(handle, FileAccess.Read, bufferSize, isAsync);
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should().Contain(nameof(MockFileSystem) + "." +
			                                nameof(MockFileSystem.WithSafeFileHandleStrategy));
	}
}
#endif
