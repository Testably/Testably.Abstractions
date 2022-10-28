using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

public sealed class MockFileSystemTests
	: ZipArchiveTests<MockFileSystem>, IDisposable
{
	/// <inheritdoc cref="ZipArchiveTests{TFileSystem}.BasePath" />
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