using System.IO;
using System.Threading;
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

		[SkippableTheory]
		[InlineAutoData(4096)]
		[InlineAutoData(8192)]
		[FileSystemTests.FileSystemWatcher(nameof(IFileSystem.IFileSystemWatcher.Error))]
		public void Error_ShouldBeTriggeredWhenBufferOverflows(
			int internalBufferSize, string path)
		{
			FileSystem.Directory.CreateDirectory(path);
			IFileSystem.IFileSystemWatcher fileSystemWatcher =
				FileSystem.FileSystemWatcher.New(BasePath);
			ManualResetEventSlim block1 = new();
			ManualResetEventSlim block2 = new();
			ErrorEventArgs? result = null;
			fileSystemWatcher.Error += (_, eventArgs) =>
			{
				result = eventArgs;
				block1.Set();
				block2.Set();
			};
			fileSystemWatcher.Deleted += (_, _) =>
			{
				block1.Wait(10000);
			};
			fileSystemWatcher.EnableRaisingEvents = true;
			fileSystemWatcher.InternalBufferSize = internalBufferSize;
			FileSystem.Directory.Delete(path);
			for (int i = 0; i <= internalBufferSize; i++)
			{
				if (block1.IsSet)
				{
					break;
				}

				FileSystem.Directory.CreateDirectory($"{i}_{path}");
			}

			block2.Wait(10000).Should().BeTrue();
			fileSystemWatcher.Dispose();
			result.Should().NotBeNull();
			result!.GetException().Should().BeOfType<InternalBufferOverflowException>();
		}
	}
}