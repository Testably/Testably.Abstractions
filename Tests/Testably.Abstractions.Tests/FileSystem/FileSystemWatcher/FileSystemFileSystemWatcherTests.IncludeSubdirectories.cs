using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

public abstract partial class FileSystemFileSystemWatcherTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void IncludeSubdirectories_SetToFalse_ShouldNotTriggerNotification(
		string baseDirectory, string path)
	{
		FileSystem.Initialize()
		   .WithSubdirectory(baseDirectory).Initialized(s => s
			   .WithSubdirectory(path));
		ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.IncludeSubdirectories = false;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(FileSystem.Path.Combine(baseDirectory, path));
		ms.Wait(30).Should().BeFalse();

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void
		IncludeSubdirectories_SetToTrue_ShouldOnlyTriggerNotificationOnSubdirectories(
			string baseDirectory, string subdirectoryName, string otherDirectory)
	{
		FileSystem.Initialize()
		   .WithSubdirectory(baseDirectory).Initialized(s => s
			   .WithSubdirectory(subdirectoryName))
		   .WithSubdirectory(otherDirectory);
		ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(baseDirectory);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(otherDirectory);
		ms.Wait(30).Should().BeFalse();

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void IncludeSubdirectories_SetToTrue_ShouldTriggerNotificationOnSubdirectories(
		string baseDirectory, string subdirectoryName)
	{
		FileSystem.Initialize()
		   .WithSubdirectory(baseDirectory).Initialized(s => s
			   .WithSubdirectory(subdirectoryName));
		string subdirectoryPath =
			FileSystem.Path.Combine(baseDirectory, subdirectoryName);
		ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(subdirectoryPath);
		ms.Wait(10000).Should().BeTrue();

		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(subdirectoryPath));
		result.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
		result.Name.Should().Be(subdirectoryPath);
	}
}