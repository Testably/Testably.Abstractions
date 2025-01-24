using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class IncludeSubdirectoriesTests
{
	[Theory]
	[AutoData]
	public void IncludeSubdirectories_SetToFalse_ShouldNotTriggerNotification(
		string baseDirectory, string path)
	{
		FileSystem.Initialize()
			.WithSubdirectory(baseDirectory).Initialized(s => s
				.WithSubdirectory(path));
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
		fileSystemWatcher.IncludeSubdirectories = false;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(FileSystem.Path.Combine(baseDirectory, path));
		ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken).Should().BeFalse();

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void
		IncludeSubdirectories_SetToTrue_ShouldOnlyTriggerNotificationOnSubdirectories(
			string baseDirectory, string subdirectoryName, string otherDirectory)
	{
		FileSystem.Initialize()
			.WithSubdirectory(baseDirectory).Initialized(s => s
				.WithSubdirectory(subdirectoryName))
			.WithSubdirectory(otherDirectory);
		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(baseDirectory);
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
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(otherDirectory);
		ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken).Should().BeFalse();

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void IncludeSubdirectories_SetToTrue_ShouldTriggerNotificationOnSubdirectories(
		string baseDirectory, string subdirectoryName)
	{
		FileSystem.Initialize()
			.WithSubdirectory(baseDirectory).Initialized(s => s
				.WithSubdirectory(subdirectoryName));
		string subdirectoryPath =
			FileSystem.Path.Combine(baseDirectory, subdirectoryName);
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
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(subdirectoryPath);
		ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken).Should().BeTrue();

		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(subdirectoryPath));
		result.Name.Should().Be(subdirectoryPath);
		result!.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
	}
}
