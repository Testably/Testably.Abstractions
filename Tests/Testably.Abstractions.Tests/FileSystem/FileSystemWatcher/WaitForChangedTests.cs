using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class WaitForChangedTests
{
	[Theory]
	[AutoData]
	public async Task WaitForChanged_ShouldBlockUntilEventHappens(string path)
	{
		SkipIfBrittleTestsShouldBeSkipped();

		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		try
		{
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						await Task.Delay(10, TestContext.Current.CancellationToken);
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, TestContext.Current.CancellationToken);

			using (CancellationTokenSource cts = new(ExpectSuccess))
			{
				cts.Token.Register(() => throw new TimeoutException());
				IWaitForChangedResult result =
					fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created);
				await That(fileSystemWatcher.EnableRaisingEvents).IsFalse();
				await That(result.TimedOut).IsFalse();
				await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Created);
				await That(result.Name).IsEqualTo(path);
				await That(result.OldName).IsNull();
			}
		}
		finally
		{
			ms.Set();
		}
	}

	[Theory]
	[MemberData(nameof(GetWaitForChangedTimeoutParameters))]
	public async Task WaitForChanged_Timeout_ShouldReturnTimedOut(string path,
		Func<IFileSystemWatcher, IWaitForChangedResult> callback)
	{
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		string fullPath = FileSystem.Path.GetFullPath(path);

		try
		{
			fileSystemWatcher.EnableRaisingEvents = true;
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						await Task.Delay(10, TestContext.Current.CancellationToken);
						FileSystem.Directory.CreateDirectory(fullPath);
						FileSystem.Directory.Delete(fullPath);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, TestContext.Current.CancellationToken);
			IWaitForChangedResult result = callback(fileSystemWatcher);

			await That(fileSystemWatcher.EnableRaisingEvents).IsTrue();
			await That(result.TimedOut).IsTrue();
			await That(result.ChangeType).IsEqualTo(0);
			await That(result.Name).IsNull();
			await That(result.OldName).IsNull();
		}
		finally
		{
			ms.Set();
		}
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<string, Func<IFileSystemWatcher, IWaitForChangedResult>>
		GetWaitForChangedTimeoutParameters()
	{
		TheoryData<string, Func<IFileSystemWatcher, IWaitForChangedResult>> theoryData = new()
		{
			{
				"foo.dll", fileSystemWatcher
					=> fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed, 100)
			},
		};
#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
		theoryData.Add(
			"bar.txt",
			fileSystemWatcher
				=> fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed,
					TimeSpan.FromMilliseconds(100))
		);
#endif
		return theoryData;
	}
	#pragma warning restore MA0018

	#endregion
}
