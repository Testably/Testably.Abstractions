using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfoFactory;

public sealed class MockFileSystemTests
	: FileSystemDriveInfoFactoryTests<MockFileSystem>, IDisposable
{
	/// <inheritdoc cref="FileSystemDriveInfoFactoryTests{TFileSystem}.BasePath" />
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