using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class NotifyFiltersTests
{
	[Theory]
	[AutoData]
	public async Task NotifyFilter_AppendFile_ShouldNotNotifyOnOtherFilters(string fileName)
	{
		// This test is brittle on MacOS
		Skip.If(Test.RunsOnMac);

		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Changed += (_, eventArgs) =>
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

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

	[Theory]
	[InlineAutoData(NotifyFilters.CreationTime)]
	[InlineAutoData(NotifyFilters.LastAccess)]
	[InlineAutoData(NotifyFilters.LastWrite)]
	[InlineAutoData(NotifyFilters.Security)]
	[InlineAutoData(NotifyFilters.Size)]
	public async Task NotifyFilter_AppendFile_ShouldTriggerChangedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string fileName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();
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
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Changed += (_, eventArgs) =>
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
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.AppendAllText(fileName, "foo");

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(fileName));
		await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Changed);
		await That(result.Name).IsEqualTo(FileSystem.Path.GetFileName(fileName));
	}

	[Theory]
	[AutoData]
	public async Task NotifyFilter_CreateDirectory_ShouldNotNotifyOnOtherFilters(string path)
	{
		// This test is brittle on MacOS
		Skip.If(Test.RunsOnMac);

		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Created += (_, eventArgs) =>
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
		fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
		                                 NotifyFilters.CreationTime |
		                                 NotifyFilters.FileName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.CreateDirectory(path);

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

	[Theory]
	[InlineAutoData(NotifyFilters.DirectoryName)]
	public async Task NotifyFilter_CreateDirectory_ShouldTriggerCreatedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string path)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Created += (_, eventArgs) =>
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
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.CreateDirectory(path);

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(path));
		await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Created);
		await That(result.Name).IsEqualTo(FileSystem.Path.GetFileName(path));
	}

	[Theory]
	[AutoData]
	public async Task NotifyFilter_CreateFile_ShouldNotNotifyOnOtherFilters(string path)
	{
		// This test is brittle on MacOS
		Skip.If(Test.RunsOnMac);

		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Created += (_, eventArgs) =>
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
		fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
		                                 NotifyFilters.CreationTime |
		                                 NotifyFilters.DirectoryName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.WriteAllText(path, "foo");

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

	[Theory]
	[InlineAutoData(NotifyFilters.FileName)]
	public async Task NotifyFilter_CreateFile_ShouldTriggerCreatedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string path)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Created += (_, eventArgs) =>
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
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.WriteAllText(path, "foo");

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(path));
		await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Created);
		await That(result.Name).IsEqualTo(FileSystem.Path.GetFileName(path));
	}

	[Theory]
	[AutoData]
	public async Task NotifyFilter_DeleteDirectory_ShouldNotNotifyOnOtherFilters(string path)
	{
		// This test is brittle on MacOS
		Skip.If(Test.RunsOnMac);

		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize().WithSubdirectory(path);
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
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
		fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
		                                 NotifyFilters.CreationTime |
		                                 NotifyFilters.FileName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Delete(path);

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

	[Theory]
	[InlineAutoData(NotifyFilters.DirectoryName)]
	public async Task NotifyFilter_DeleteDirectory_ShouldTriggerDeletedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string path)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize().WithSubdirectory(path);
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
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
		fileSystemWatcher.NotifyFilter = notifyFilter;
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
	public async Task NotifyFilter_DeleteFile_ShouldNotNotifyOnOtherFilters(string path)
	{
		// This test is brittle on MacOS
		Skip.If(Test.RunsOnMac);
		
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize().WithFile(path);
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
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
		fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
		                                 NotifyFilters.CreationTime |
		                                 NotifyFilters.DirectoryName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Delete(path);

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

	[Theory]
	[InlineAutoData(NotifyFilters.FileName)]
	public async Task NotifyFilter_DeleteFile_ShouldTriggerDeletedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string path)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize().WithFile(path);
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
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
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Delete(path);

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(path));
		await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Deleted);
		await That(result.Name).IsEqualTo(FileSystem.Path.GetFileName(path));
	}

	[Theory]
	[AutoData]
	public async Task NotifyFilter_MoveFile_DifferentDirectories_ShouldNotifyOnLinuxOrMac(
		string sourcePath, string sourceName,
		string destinationPath, string destinationName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();
		Skip.If(Test.RunsOnWindows);

		FileSystem.Initialize()
			.WithSubdirectory(sourcePath).Initialized(s => s
				.WithFile(sourceName))
			.WithSubdirectory(destinationPath);
		RenamedEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
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

		FileSystem.File.Move(
			FileSystem.Path.Combine(sourcePath, sourceName),
			FileSystem.Path.Combine(destinationPath, destinationName));

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.ChangeType).IsEqualTo(WatcherChangeTypes.Renamed);
		await That(result.FullPath)
			.IsEqualTo(FileSystem.Path.Combine(BasePath, destinationPath, destinationName));
		await That(result.Name)
			.IsEqualTo(FileSystem.Path.Combine(destinationPath, destinationName));
		await That(result.OldFullPath)
			.IsEqualTo(FileSystem.Path.Combine(BasePath, sourcePath, sourceName));
		await That(result.OldName).IsEqualTo(FileSystem.Path.Combine(sourcePath, sourceName));
	}

	[Theory]
	[AutoData]
	public async Task NotifyFilter_MoveFile_DifferentDirectories_ShouldNotNotify_OnWindows(
		string sourcePath, string sourceName,
		string destinationPath, string destinationName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Initialize()
			.WithSubdirectory(sourcePath).Initialized(s => s
				.WithFile(sourceName))
			.WithSubdirectory(destinationPath);
		RenamedEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
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

		FileSystem.File.Move(
			FileSystem.Path.Combine(sourcePath, sourceName),
			FileSystem.Path.Combine(destinationPath, destinationName));

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task NotifyFilter_MoveFile_ShouldNotNotifyOnOtherFilters(
		string sourceName, string destinationName)
	{
		// This test is brittle on MacOS
		Skip.If(Test.RunsOnMac);

		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystem.File.WriteAllText(sourceName, null);
		RenamedEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
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
		fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
		                                 NotifyFilters.CreationTime |
		                                 NotifyFilters.DirectoryName |
		                                 NotifyFilters.LastAccess |
		                                 NotifyFilters.LastWrite |
		                                 NotifyFilters.Security |
		                                 NotifyFilters.Size;

		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Move(sourceName, destinationName);

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

	[Theory]
	[InlineAutoData(NotifyFilters.FileName)]
	public async Task NotifyFilter_MoveFile_ShouldTriggerChangedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string sourceName, string destinationName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystem.File.WriteAllText(sourceName, "foo");
		RenamedEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
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

		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.Move(sourceName, destinationName);

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.ChangeType).IsEqualTo(WatcherChangeTypes.Renamed);
		await That(result.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(result.Name).IsEqualTo(FileSystem.Path.GetFileName(destinationName));
		await That(result.OldFullPath).IsEqualTo(FileSystem.Path.GetFullPath(sourceName));
		await That(result.OldName).IsEqualTo(FileSystem.Path.GetFileName(sourceName));
	}

	[Theory]
	[InlineAutoData(NotifyFilters.DirectoryName)]
	public async Task NotifyFilter_MoveDirectory_ShouldTriggerChangedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string sourceName, string destinationName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystem.Directory.CreateDirectory(sourceName);
		RenamedEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Renamed += (_, eventArgs) =>
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

		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(sourceName, destinationName);

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.ChangeType).IsEqualTo(WatcherChangeTypes.Renamed);
		await That(result.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(result.Name).IsEqualTo(FileSystem.Path.GetFileName(destinationName));
		await That(result.OldFullPath).IsEqualTo(FileSystem.Path.GetFullPath(sourceName));
		await That(result.OldName).IsEqualTo(FileSystem.Path.GetFileName(sourceName));
	}

	[Theory]
	[InlineAutoData(NotifyFilters.DirectoryName)]
	public async Task NotifyFilter_MoveDirectoryOutOfTheWatchedDirectory_ShouldTriggerChangedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string sourceName, string destinationName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize().WithSubdirectory("watched");
		var sourcePath = FileSystem.Path.Combine("watched", sourceName);
		FileSystem.Directory.CreateDirectory(sourcePath);
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New("watched");
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

		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(sourcePath, destinationName);

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.ChangeType).IsEqualTo(WatcherChangeTypes.Deleted);
		await That(result.FullPath).IsEqualTo(sourcePath);
		await That(result.Name).IsEqualTo(sourceName);
	}

	[Theory]
	[AutoData]
	public async Task NotifyFilter_WriteFile_ShouldNotNotifyOnOtherFilters(string fileName)
	{
		// This test is brittle on MacOS
		Skip.If(Test.RunsOnMac);

		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Initialize();
		FileSystem.File.WriteAllText(fileName, null);
		FileSystemEventArgs? result = null;
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Changed += (_, eventArgs) =>
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

		await That(ms.Wait(EnsureTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

	[Theory]
	[InlineAutoData(NotifyFilters.CreationTime)]
	[InlineAutoData(NotifyFilters.LastAccess)]
	[InlineAutoData(NotifyFilters.LastWrite)]
	[InlineAutoData(NotifyFilters.Security)]
	[InlineAutoData(NotifyFilters.Size)]
	public async Task NotifyFilter_WriteFile_ShouldTriggerChangedEventOnNotifyFilters(
		NotifyFilters notifyFilter, string fileName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

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
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Changed += (_, eventArgs) =>
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
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.File.WriteAllText(fileName, "foo");

		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(fileName));
		await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Changed);
		await That(result.Name).IsEqualTo(FileSystem.Path.GetFileName(fileName));
	}
}
