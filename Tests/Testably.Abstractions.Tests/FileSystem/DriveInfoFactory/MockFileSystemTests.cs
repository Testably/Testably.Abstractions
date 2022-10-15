using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfoFactory;

[SystemTest(nameof(MockFileSystemTests))]
public sealed class MockFileSystemTests
	: FileSystemDriveInfoFactoryTests<FileSystemMock>, IDisposable
{
	/// <inheritdoc cref="FileSystemDriveInfoFactoryTests{TFileSystem}.BasePath" />
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