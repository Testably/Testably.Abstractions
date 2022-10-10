using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(MockFileSystem))]
	public sealed class FileStreamTests
		: FileSystemFileStreamTests<FileSystemMock>, IDisposable
	{
		/// <inheritdoc cref="FileSystemFileStreamTests{TFileSystem}.BasePath" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

		public FileStreamTests() : this(new FileSystemMock())
		{
		}

		private FileStreamTests(FileSystemMock fileSystemMock) : base(
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