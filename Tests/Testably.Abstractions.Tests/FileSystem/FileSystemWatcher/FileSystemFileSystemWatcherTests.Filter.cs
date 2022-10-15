using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

public abstract partial class FileSystemFileSystemWatcherTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.Filter))]
	public void Filter_Matching_ShouldTriggerNotification(string path)
	{
		FileSystem.Initialize().WithSubdirectory(path);
		ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.Filter = path;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(path);
		ms.Wait(10000).Should().BeTrue();

		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.Filter))]
	public void Filters_ShouldMatchAnyOfTheSpecifiedFilters(
		string[] filteredPaths, string[] otherPaths)
	{
		foreach (string path in otherPaths.Concat(filteredPaths))
		{
			FileSystem.Directory.CreateDirectory(path);
		}

		CountdownEvent ms = new(filteredPaths.Length);
		List<FileSystemEventArgs> results = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
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

		ms.Wait(10000).Should().BeTrue();

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

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.Filter))]
	public void Filter_NotMatching_ShouldNotTriggerNotification(
		string path, string filter)
	{
		FileSystem.Initialize().WithSubdirectory(path);
		ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.Filter = filter;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(path);
		ms.Wait(30).Should().BeFalse();

		result.Should().BeNull();
	}
}