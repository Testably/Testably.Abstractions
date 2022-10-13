using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(MockFileSystem))]
	public sealed class FileSystemWatcherTests
		: FileSystemFileSystemWatcherTests<FileSystemMock>, IDisposable
	{
		/// <inheritdoc cref="FileSystemFileSystemWatcherTests{TFileSystem}.BasePath" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

		public FileSystemWatcherTests() : this(new FileSystemMock())
		{
		}

		private FileSystemWatcherTests(FileSystemMock fileSystemMock) : base(
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
}