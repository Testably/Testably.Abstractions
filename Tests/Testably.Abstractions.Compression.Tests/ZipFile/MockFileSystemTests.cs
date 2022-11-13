using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Compression.Tests.ZipFile;

// ReSharper disable once UnusedMember.Global
public sealed class MockFileSystemTests
	: ZipFileTests<MockFileSystem>, IDisposable
{
	/// <inheritdoc cref="ZipFileTests{TFileSystem}.BasePath" />
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