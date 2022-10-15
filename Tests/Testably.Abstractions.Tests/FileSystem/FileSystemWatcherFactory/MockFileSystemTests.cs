using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcherFactory;

[SystemTest(nameof(DriveInfoFactory.MockFileSystemTests))]
public sealed class MockFileSystemTests
	: FileSystemFileSystemWatcherFactoryTests<FileSystemMock>, IDisposable
{
	/// <inheritdoc cref="FileSystemFileSystemWatcherFactoryTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

	public MockFileSystemTests() : this(new FileSystemMock())
	{
	}

	private MockFileSystemTests(FileSystemMock fileSystemMock) : base(
		fileSystemMock,
		fileSystemMock.TimeSystem)
	{
		_directoryCleaner = FileSystem
		   .SetCurrentDirectoryToEmptyTemporaryDirectory();
	}

	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion
}