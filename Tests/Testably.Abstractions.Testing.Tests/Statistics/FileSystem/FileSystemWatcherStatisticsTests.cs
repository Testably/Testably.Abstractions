using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileSystemWatcherStatisticsTests
{
	[SkippableFact]
	public void BeginInit_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.FileSystemWatcher.New("foo").BeginInit();

		sut.Statistics.FileSystemWatcher["foo"].ShouldOnlyContain(nameof(IFileSystemWatcher.BeginInit));
	}

	[SkippableFact]
	public void EndInit_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");

		sut.FileSystemWatcher.New("foo").EndInit();

		sut.Statistics.FileSystemWatcher["foo"].ShouldOnlyContain(nameof(IFileSystemWatcher.EndInit));
	}

	[SkippableFact]
	public void WaitForChanged_WatcherChangeTypes_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		CancellationTokenSource cts = new(TimeSpan.FromSeconds(1));
		_ = Task.Run(async () =>
		{
			while (!cts.Token.IsCancellationRequested)
			{
				await Task.Delay(10, cts.Token);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, cts.Token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;

		sut.FileSystemWatcher.New("foo").WaitForChanged(changeType);

		sut.Statistics.FileSystemWatcher["foo"].ShouldOnlyContain(nameof(IFileSystemWatcher.WaitForChanged),
			changeType);
	}

	[SkippableFact]
	public void WaitForChanged_WatcherChangeTypes_Int_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		CancellationTokenSource cts = new(TimeSpan.FromSeconds(1));
		_ = Task.Run(async () =>
		{
			while (!cts.Token.IsCancellationRequested)
			{
				await Task.Delay(10, cts.Token);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, cts.Token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;
		int timeout = 42;

		sut.FileSystemWatcher.New("foo").WaitForChanged(changeType, timeout);

		sut.Statistics.FileSystemWatcher["foo"].ShouldOnlyContain(nameof(IFileSystemWatcher.WaitForChanged),
			changeType, timeout);
	}

#if FEATURE_FILESYSTEM_NET7
	[SkippableFact]
	public void WaitForChanged_WatcherChangeTypes_TimeSpan_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		// Changes in the background are necessary, so that FileSystemWatcher.WaitForChanged returns.
		CancellationTokenSource cts = new(TimeSpan.FromSeconds(1));
		_ = Task.Run(async () =>
		{
			while (!cts.Token.IsCancellationRequested)
			{
				await Task.Delay(10, cts.Token);
				sut.Directory.CreateDirectory(sut.Path.Combine("foo", "some-directory"));
				sut.Directory.Delete(sut.Path.Combine("foo", "some-directory"));
			}
		}, cts.Token);
		WatcherChangeTypes changeType = WatcherChangeTypes.Created;
		TimeSpan timeout = TimeSpan.FromSeconds(2);

		sut.FileSystemWatcher.New("foo").WaitForChanged(changeType, timeout);

		sut.Statistics.FileSystemWatcher["foo"].ShouldOnlyContain(nameof(IFileSystemWatcher.WaitForChanged),
			changeType, timeout);
	}
#endif
}
