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
	public void Filter_Matching_ShouldTriggerNotification(string path)
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
		ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken).Should().BeTrue();

		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}

	[Theory]
	[AutoData]
	public void Filter_NotMatching_ShouldNotTriggerNotification(
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
		ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken).Should().BeFalse();

		result.Should().BeNull();
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[Theory]
	[AutoData]
	public void Filters_ShouldMatchAnyOfTheSpecifiedFilters(
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

		ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken).Should().BeTrue();

		foreach (string path in otherPaths)
		{
			results.Should()
				.NotContain(f => f.FullPath == FileSystem.Path.GetFullPath(path));
		}

		foreach (string path in filteredPaths)
		{
			results.Should()
				.Contain(f => f.FullPath == FileSystem.Path.GetFullPath(path));
		}
	}
#endif
}
