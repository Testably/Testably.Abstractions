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
		try
		{
			new Thread(() =>
			{
				while (!ms.IsSet)
				{
					Thread.Sleep(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			}).Start();

			using (CancellationTokenSource cts = new(5000))
			{
				cts.Token.Register(() => throw new TimeoutException());
				IFileSystem.IFileSystemWatcher.IWaitForChangedResult result =
					fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created);
				fileSystemWatcher.EnableRaisingEvents.Should().BeFalse();
				result.TimedOut.Should().BeFalse();
				result.ChangeType.Should().Be(WatcherChangeTypes.Created);
				result.Name.Should().Be(path);
				result.OldName.Should().BeNull();
			}
		}
		finally
		{
			ms.Set();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void WaitForChanged_Timeout_ShouldReturnTimedOut(string path)
	{
		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		try
		{
			fileSystemWatcher.EnableRaisingEvents = true;
			new Thread(() =>
			{
				while (!ms.IsSet)
				{
					Thread.Sleep(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			}).Start();
			IFileSystem.IFileSystemWatcher.IWaitForChangedResult result =
				fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed, 100);

			fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
			result.TimedOut.Should().BeTrue();
			result.ChangeType.Should().Be(0);
			result.Name.Should().BeNull();
			result.OldName.Should().BeNull();
		}
		finally
		{
			ms.Set();
		}
	}
}