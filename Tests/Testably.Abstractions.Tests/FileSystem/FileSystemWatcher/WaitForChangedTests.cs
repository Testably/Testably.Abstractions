using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WaitForChangedTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void WaitForChanged_ShouldBlockUntilEventHappens(string path)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		try
		{
			Task.Run(() =>
			{
				while (!ms.IsSet)
				{
					Thread.Sleep(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});

			using (CancellationTokenSource cts = new(5000))
			{
				cts.Token.Register(() => throw new TimeoutException());
				IFileSystemWatcher.IWaitForChangedResult result =
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
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		try
		{
			fileSystemWatcher.EnableRaisingEvents = true;
			Task.Run(() =>
			{
				while (!ms.IsSet)
				{
					Thread.Sleep(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});
			IFileSystemWatcher.IWaitForChangedResult result =
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
