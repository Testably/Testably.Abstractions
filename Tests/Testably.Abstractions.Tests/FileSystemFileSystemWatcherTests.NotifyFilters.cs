using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileSystemWatcherTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.NotifyFilter))]
	public void NotifyFilter_CreateDirectory_ShouldNotNotifyOnOtherFilters(string path)
	{
		FileSystem.Initialize();
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
		                                 NotifyFilters.CreationTime |
		                                 NotifyFilters.FileName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.CreateDirectory(path);

		ms.Wait(500).Should().BeFalse();
		result.Should().BeNull();
	}

	[SkippableTheory]
	[InlineAutoData(NotifyFilters.DirectoryName)]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.NotifyFilter))]
	public void NotifyFilter_CreateDirectory_ShouldTriggerCreatedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string path)
	{
		FileSystem.Initialize();
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.CreateDirectory(path);

		ms.Wait(5000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Created);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.NotifyFilter))]
	public void NotifyFilter_DeleteDirectory_ShouldNotNotifyOnOtherFilters(string path)
	{
		FileSystem.Initialize().WithSubdirectory(path);
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
		                                 NotifyFilters.CreationTime |
		                                 NotifyFilters.FileName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Delete(path);

		ms.Wait(500).Should().BeFalse();
		result.Should().BeNull();
	}

	[SkippableTheory]
	[InlineAutoData(NotifyFilters.DirectoryName)]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.NotifyFilter))]
	public void NotifyFilter_DeleteDirectory_ShouldTriggerDeletedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string path)
	{
		FileSystem.Initialize().WithSubdirectory(path);
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Delete(path);

		ms.Wait(5000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.NotifyFilter))]
	public void NotifyFilter_WriteFile_ShouldNotNotifyOnOtherFilters(string fileName)
	{
		FileSystem.Initialize();
		FileSystem.File.WriteAllText(fileName, null);
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Changed += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName |
		                                 NotifyFilters.FileName;
		if (!Test.RunsOnMac)
		{
			fileSystemWatcher.NotifyFilter |= NotifyFilters.CreationTime;
		}

		if (!Test.RunsOnLinux)
		{
			fileSystemWatcher.NotifyFilter |= NotifyFilters.Security;
		}
		else
		{
			fileSystemWatcher.NotifyFilter |= NotifyFilters.Attributes;
		}

		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.WriteAllText(fileName, "foo");

		ms.Wait(500).Should().BeFalse();
		result.Should().BeNull();
	}

	[SkippableTheory]
	[InlineAutoData(NotifyFilters.Attributes)]
	[InlineAutoData(NotifyFilters.CreationTime)]
	[InlineAutoData(NotifyFilters.LastAccess)]
	[InlineAutoData(NotifyFilters.LastWrite)]
	[InlineAutoData(NotifyFilters.Security)]
	[InlineAutoData(NotifyFilters.Size)]
	[FileSystemTests.FileSystemWatcher(
		nameof(IFileSystem.IFileSystemWatcher.NotifyFilter))]
	public void NotifyFilter_WriteFile_ShouldTriggerChangedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string fileName)
	{
		if (!Test.RunsOnLinux)
		{
			Skip.If(notifyFilter == NotifyFilters.Security,
				"`Security` is only set on Linux");
		}

		if (!Test.RunsOnMac)
		{
			Skip.If(notifyFilter == NotifyFilters.CreationTime,
				"`CreationTime` is only set on MAC");
		}

		if (Test.RunsOnWindows)
		{
			Skip.If(notifyFilter == NotifyFilters.LastAccess,
				"`LastAccess` is not consistently set on the real file system.");
		}
		else
		{
			Skip.If(notifyFilter == NotifyFilters.Attributes,
				"`Attributes` is only set on Windows");
		}

		FileSystem.Initialize();
		FileSystem.File.WriteAllText(fileName, null);
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Changed += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.WriteAllText(fileName, "foo");

		ms.Wait(5000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(fileName));
		result.ChangeType.Should().Be(WatcherChangeTypes.Changed);
		result.Name.Should().Be(FileSystem.Path.GetFileName(fileName));
	}
}