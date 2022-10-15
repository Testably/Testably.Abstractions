using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

public abstract partial class FileSystemFileSystemWatcherTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void NotifyFilter_AppendFile_ShouldNotNotifyOnOtherFilters(string fileName)
	{
		FileSystem.Initialize();
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

		FileSystem.File.AppendAllText(fileName, "foo");

		ms.Wait(500).Should().BeFalse();
		result.Should().BeNull();
	}

	[SkippableTheory]
	[InlineAutoData(NotifyFilters.CreationTime)]
	[InlineAutoData(NotifyFilters.LastAccess)]
	[InlineAutoData(NotifyFilters.LastWrite)]
	[InlineAutoData(NotifyFilters.Security)]
	[InlineAutoData(NotifyFilters.Size)]
	public void NotifyFilter_AppendFile_ShouldTriggerChangedEventOnNotifyFilters(
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

		FileSystem.File.AppendAllText(fileName, "foo");

		ms.Wait(3000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(fileName));
		result.ChangeType.Should().Be(WatcherChangeTypes.Changed);
		result.Name.Should().Be(FileSystem.Path.GetFileName(fileName));
	}

	[SkippableTheory]
	[AutoData]
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

		ms.Wait(3000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Created);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}

	[SkippableTheory]
	[AutoData]
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

		ms.Wait(3000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}

	[SkippableTheory]
	[AutoData]
	public void NotifyFilter_DeleteFile_ShouldNotNotifyOnOtherFilters(string path)
	{
		FileSystem.Initialize().WithFile(path);
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
		                                 NotifyFilters.DirectoryName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Delete(path);

		ms.Wait(500).Should().BeFalse();
		result.Should().BeNull();
	}

	[SkippableTheory]
	[InlineAutoData(NotifyFilters.FileName)]
	public void NotifyFilter_DeleteFile_ShouldTriggerDeletedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string path)
	{
		FileSystem.Initialize().WithFile(path);
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

		FileSystem.File.Delete(path);

		ms.Wait(3000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(path));
		result.ChangeType.Should().Be(WatcherChangeTypes.Deleted);
		result.Name.Should().Be(FileSystem.Path.GetFileName(path));
	}

	[SkippableTheory]
	[AutoData]
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
	[InlineAutoData(NotifyFilters.CreationTime)]
	[InlineAutoData(NotifyFilters.LastAccess)]
	[InlineAutoData(NotifyFilters.LastWrite)]
	[InlineAutoData(NotifyFilters.Security)]
	[InlineAutoData(NotifyFilters.Size)]
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

		FileSystem.Initialize();
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

		ms.Wait(3000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.FullPath.Should().Be(FileSystem.Path.GetFullPath(fileName));
		result.ChangeType.Should().Be(WatcherChangeTypes.Changed);
		result.Name.Should().Be(FileSystem.Path.GetFileName(fileName));
	}

	[SkippableTheory]
	[AutoData]
	public void NotifyFilter_MoveFile_ShouldNotNotifyOnOtherFilters(
		string sourceName, string destinationName)
	{
		FileSystem.Initialize();
		FileSystem.File.WriteAllText(sourceName, null);
		RenamedEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
		                                 NotifyFilters.CreationTime |
		                                 NotifyFilters.DirectoryName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;

		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Move(sourceName, destinationName);

		ms.Wait(500).Should().BeFalse();
		result.Should().BeNull();
	}

	[SkippableTheory]
	[InlineAutoData(NotifyFilters.FileName)]
	public void NotifyFilter_MoveFile_ShouldTriggerChangedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string sourceName, string destinationName)
	{
		FileSystem.Initialize();
		FileSystem.File.WriteAllText(sourceName, "foo");
		RenamedEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};

		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Move(sourceName, destinationName);

		ms.Wait(2000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
		result.FullPath.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		result.Name.Should().Be(FileSystem.Path.GetFileName(destinationName));
		result.OldFullPath.Should().Be(FileSystem.Path.GetFullPath(sourceName));
		result.OldName.Should().Be(FileSystem.Path.GetFileName(sourceName));
	}

	[SkippableTheory]
	[InlineAutoData(NotifyFilters.FileName)]
	public void
		NotifyFilter_MoveFile_DifferentDirectories_ShouldNotNotifyOnWindows(
			NotifyFilters notifyFilter, string sourcePath, string sourceName,
			string destinationPath, string destinationName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Initialize()
		   .WithSubdirectory(sourcePath).Initialized(s => s
			   .WithFile(sourceName))
		   .WithSubdirectory(destinationPath);
		RenamedEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};

		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Move(
			FileSystem.Path.Combine(sourcePath, sourceName),
			FileSystem.Path.Combine(destinationPath, destinationName));

		ms.Wait(500).Should().BeFalse();
		result.Should().BeNull();
	}

	[SkippableTheory]
	[InlineAutoData(NotifyFilters.FileName)]
	public void
		NotifyFilter_MoveFile_DifferentDirectories_ShouldNotifyOnLinuxOrMac(
			NotifyFilters notifyFilter, string sourcePath, string sourceName,
			string destinationPath, string destinationName)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.Initialize()
		   .WithSubdirectory(sourcePath).Initialized(s => s
			   .WithFile(sourceName))
		   .WithSubdirectory(destinationPath);
		RenamedEventArgs? result = null;
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};

		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Move(
			FileSystem.Path.Combine(sourcePath, sourceName),
			FileSystem.Path.Combine(destinationPath, destinationName));

		ms.Wait(2000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
		result.FullPath.Should()
		   .Be(FileSystem.Path.Combine(BasePath, destinationPath, destinationName));
		result.Name.Should()
		   .Be(FileSystem.Path.Combine(destinationPath, destinationName));
		result.OldFullPath.Should()
		   .Be(FileSystem.Path.Combine(BasePath, sourcePath, sourceName));
		result.OldName.Should().Be(FileSystem.Path.Combine(sourcePath, sourceName));
	}
}