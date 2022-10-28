using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

public sealed class MockFileSystemTests
	: FileSystemDriveInfoTests<MockFileSystem>, IDisposable
{
	/// <inheritdoc cref="FileSystemDriveInfoTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly IDirectoryCleaner _directoryCleaner;

	public MockFileSystemTests() : this(new MockFileSystem())
	{
	}

	private MockFileSystemTests(MockFileSystem mockFileSystem) : base(
		mockFileSystem,
		mockFileSystem.TimeSystem)
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