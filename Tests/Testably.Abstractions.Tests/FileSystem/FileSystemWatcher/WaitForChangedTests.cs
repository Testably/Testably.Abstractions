using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public class WaitForChangedTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	#pragma warning disable MA0018
	public static IEnumerable<(string, Func<IFileSystemWatcher, IWaitForChangedResult>)>
		GetWaitForChangedTimeoutParameters()
	{
		yield return ("foo.dll", fileSystemWatcher
			=> fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed, 100));
#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
		yield return ("bar.txt",
				fileSystemWatcher
					=> fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed,
						TimeSpan.FromMilliseconds(100))
			);
#endif
	}
	#pragma warning restore MA0018
	[Test]
	[AutoArguments]
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
						await Task.Delay(10, CancellationToken);
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, CancellationToken);

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

	[Test]
	[MethodDataSource(nameof(GetWaitForChangedTimeoutParameters))]
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
						await Task.Delay(10, CancellationToken);
						FileSystem.Directory.CreateDirectory(fullPath);
						FileSystem.Directory.Delete(fullPath);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, CancellationToken);
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

	[Test]
	public async Task WaitForChanged_WithoutPath_ShouldFailToStart()
	{
		List<Exception> errors = [];
		IWaitForChangedResult? result = null;
		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New();
		fileSystemWatcher.Error += (_, e) => errors.Add(e.GetException());

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			result = fileSystemWatcher.WaitForChanged(WatcherChangeTypes.All, 100);
		});

		if (Test.RunsOnLinux)
		{
			await That(exception).IsNull();
			await That(result!.TimedOut).IsTrue();
			await That(errors).HasSingle()
				.Because("inotify_add_watch fails for the empty path");
			await That(fileSystemWatcher.EnableRaisingEvents).IsFalse();
		}
		else if (Test.IsNetFramework)
		{
			await That(exception).IsExactly<ArgumentException>();
			await That(fileSystemWatcher.EnableRaisingEvents).IsTrue()
				.Because("the exception skips restoring the previous value");
		}
		else if (Test.RunsOnMac)
		{
			await That(exception).IsExactly<ArgumentException>();
			await That(fileSystemWatcher.EnableRaisingEvents).IsFalse();
		}
		else
		{
			await That(exception).IsExactly<FileNotFoundException>();
			await That(fileSystemWatcher.EnableRaisingEvents).IsFalse();
		}
	}
}
