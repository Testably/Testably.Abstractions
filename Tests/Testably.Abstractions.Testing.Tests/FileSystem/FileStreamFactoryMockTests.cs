#if EXECUTE_SAFEFILEHANDLE_TESTS
using Microsoft.Win32.SafeHandles;
using System.IO;
using Testably.Abstractions.Testing.FileSystemInitializer;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public sealed class FileStreamFactoryMockTests : IDisposable
{
	public string BasePath => _directoryCleaner.BasePath;
	public MockFileSystem MockFileSystem { get; }
	public RealFileSystem RealFileSystem { get; }
	private readonly IDirectoryCleaner _directoryCleaner;

	public FileStreamFactoryMockTests()
	{
		RealFileSystem = new RealFileSystem();
		_directoryCleaner = RealFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
		MockFileSystem = new MockFileSystem();
		MockFileSystem.InitializeIn(_directoryCleaner.BasePath);
	}

	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

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
