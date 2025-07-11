using System.IO;
using System.Threading;
#if FEATURE_FILESYSTEMWATCHER_ADVANCED
using System.Collections.Generic;
using System.Linq;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class FilterTests
{
	[Theory]
	[AutoData]
	public async Task Filter_Matching_ShouldTriggerNotification(string path)
	{
		FileSystem.Initialize().WithSubdirectory(path);
		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.Filter = path;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(path);
		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();

		await That(result).IsNotNull();
		await That(result!.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(path));
		await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Deleted);
		await That(result.Name).IsEqualTo(FileSystem.Path.GetFileName(path));
	}

	[Theory]
	[AutoData]
	public async Task Filter_NotMatching_ShouldNotTriggerNotification(
		string path, string filter)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize().WithSubdirectory(path);
		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.Filter = filter;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(path);
		await That(ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await That(result).IsNull();
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[Theory]
	[AutoData]
	public async Task Filters_ShouldMatchAnyOfTheSpecifiedFilters(
		string[] filteredPaths, string[] otherPaths)
	{
		foreach (string path in otherPaths.Concat(filteredPaths))
		{
			FileSystem.Directory.CreateDirectory(path);
		}

		CountdownEvent ms = new(filteredPaths.Length);
		List<FileSystemEventArgs> results = [];
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			results.Add(eventArgs);
			ms.Signal();
		};
		foreach (string filter in filteredPaths)
		{
			fileSystemWatcher.Filters.Add(filter);
		}

		fileSystemWatcher.EnableRaisingEvents = true;
		foreach (string path in otherPaths.Concat(filteredPaths))
		{
			FileSystem.Directory.Delete(path);
		}

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();

		foreach (string path in otherPaths)
		{
			await That(results).DoesNotContain(f => string.Equals(
				f.FullPath,
				FileSystem.Path.GetFullPath(path),
				StringComparison.Ordinal));
		}

		foreach (string path in filteredPaths)
		{
			await That(results).Contains(f => string.Equals(
				f.FullPath,
				FileSystem.Path.GetFullPath(path),
				StringComparison.Ordinal));
		}
	}
#endif
}
