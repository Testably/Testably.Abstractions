using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileSystemWatcherTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileSystemWatcher("Trigger")]
	public void DeleteDirectory_ShouldTriggerDeletedEvent(string path)
	{
		FileSystem.Initialize().WithSubdirectory(path);
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Delete(path);

		ms.Wait(10000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileSystemWatcher("Trigger")]
	public void CreateDirectory_ShouldTriggerCreatedEvent(string path)
	{
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.CreateDirectory(path);

		ms.Wait(10000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Created);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}
}