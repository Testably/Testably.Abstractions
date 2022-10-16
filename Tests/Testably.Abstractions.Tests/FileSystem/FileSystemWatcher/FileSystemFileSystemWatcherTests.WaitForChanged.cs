using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

public abstract partial class FileSystemFileSystemWatcherTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void WaitForChanged_ShouldBlockUntilEventHappens(string path)
	{
		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		new Thread(() =>
		{
			while (!ms.IsSet)
			{
				TimeSystem.Thread.Sleep(10);
				FileSystem.Directory.CreateDirectory(path);
				FileSystem.Directory.Delete(path);
			}
		}).Start();

		IFileSystem.IFileSystemWatcher.IWaitForChangedResult result =
			fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created);

		ms.Set();
		fileSystemWatcher.EnableRaisingEvents.Should().BeFalse();
		result.TimedOut.Should().BeFalse();
		result.ChangeType.Should().Be(WatcherChangeTypes.Created);
		result.Name.Should().Be(path);
		result.OldName.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void WaitForChanged_Timeout_ShouldReturnTimedOut(string path)
	{
		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;
		new Thread(() =>
		{
			while (!ms.IsSet)
			{
				TimeSystem.Thread.Sleep(10);
				FileSystem.Directory.CreateDirectory(path);
				FileSystem.Directory.Delete(path);
			}
		}).Start();
		IFileSystem.IFileSystemWatcher.IWaitForChangedResult result =
			fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed, 100);

		ms.Set();
		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		result.TimedOut.Should().BeTrue();
		result.ChangeType.Should().Be(0);
		result.Name.Should().BeNull();
		result.OldName.Should().BeNull();
	}
}