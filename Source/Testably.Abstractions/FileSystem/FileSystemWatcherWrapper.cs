using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#if FEATURE_FILESYSTEMWATCHER_ADVANCED
using System.Collections.ObjectModel;
#endif

namespace Testably.Abstractions.FileSystem;

internal sealed class FileSystemWatcherWrapper : IFileSystemWatcher
{
	private readonly FileSystemWatcher _instance;

	private FileSystemWatcherWrapper(FileSystemWatcher fileSystemWatcher,
		IFileSystem fileSystem)
	{
		_instance = fileSystemWatcher;
		FileSystem = fileSystem;
	}

	#region IFileSystemWatcher Members

	/// <inheritdoc cref="IFileSystemWatcher.Container" />
	public IContainer? Container
		=> _instance.Container;

	/// <inheritdoc cref="IFileSystemWatcher.EnableRaisingEvents" />
	public bool EnableRaisingEvents
	{
		get => _instance.EnableRaisingEvents;
		set => _instance.EnableRaisingEvents = value;
	}

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IFileSystemWatcher.Filter" />
	public string Filter
	{
		get => _instance.Filter;
		set => _instance.Filter = value;
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	/// <inheritdoc cref="IFileSystemWatcher.Filters" />
	public Collection<string> Filters
		=> _instance.Filters;
#endif

	/// <inheritdoc cref="IFileSystemWatcher.IncludeSubdirectories" />
	public bool IncludeSubdirectories
	{
		get => _instance.IncludeSubdirectories;
		set => _instance.IncludeSubdirectories = value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.InternalBufferSize" />
	public int InternalBufferSize
	{
		get => _instance.InternalBufferSize;
		set => _instance.InternalBufferSize = value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.NotifyFilter" />
	public NotifyFilters NotifyFilter
	{
		get => _instance.NotifyFilter;
		set => _instance.NotifyFilter = value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.Path" />
	public string Path
	{
		get => _instance.Path;
		set => _instance.Path = value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.Site" />
	public ISite? Site
	{
		get => _instance.Site;
		set => _instance.Site = value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.SynchronizingObject" />
	public ISynchronizeInvoke? SynchronizingObject
	{
		get => _instance.SynchronizingObject;
		set => _instance.SynchronizingObject = value;
	}

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _instance.Dispose();

	/// <inheritdoc cref="IFileSystemWatcher.BeginInit()" />
	public void BeginInit()
		=> _instance.BeginInit();

	/// <inheritdoc cref="IFileSystemWatcher.Changed" />
	public event FileSystemEventHandler? Changed
	{
		add => _instance.Changed += value;
		remove => _instance.Changed -= value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.Created" />
	public event FileSystemEventHandler? Created
	{
		add => _instance.Created += value;
		remove => _instance.Created -= value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.Deleted" />
	public event FileSystemEventHandler? Deleted
	{
		add => _instance.Deleted += value;
		remove => _instance.Deleted -= value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.EndInit()" />
	public void EndInit()
		=> _instance.EndInit();

	/// <inheritdoc cref="IFileSystemWatcher.Error" />
	public event ErrorEventHandler? Error
	{
		add => _instance.Error += value;
		remove => _instance.Error -= value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.Renamed" />
	public event RenamedEventHandler? Renamed
	{
		add => _instance.Renamed += value;
		remove => _instance.Renamed -= value;
	}

	/// <inheritdoc cref="IFileSystemWatcher.WaitForChanged(WatcherChangeTypes)" />
	public IWaitForChangedResult WaitForChanged(
		WatcherChangeTypes changeType)
		=> new WaitForChangedResultWrapper(_instance.WaitForChanged(changeType));

	/// <inheritdoc cref="IFileSystemWatcher.WaitForChanged(WatcherChangeTypes, int)" />
	public IWaitForChangedResult WaitForChanged(
		WatcherChangeTypes changeType, int timeout)
		=> new WaitForChangedResultWrapper(
			_instance.WaitForChanged(changeType, timeout));

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IFileSystemWatcher.WaitForChanged(WatcherChangeTypes, TimeSpan)" />
	public IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType,
		TimeSpan timeout)
		=> new WaitForChangedResultWrapper(
			_instance.WaitForChanged(changeType, timeout));
#endif

	#endregion

	[return: NotNullIfNotNull("instance")]
	internal static FileSystemWatcherWrapper? FromFileSystemWatcher(
		FileSystemWatcher? instance, IFileSystem fileSystem)
	{
		if (instance == null)
		{
			return null;
		}

		return new FileSystemWatcherWrapper(instance, fileSystem);
	}

	private readonly struct WaitForChangedResultWrapper
		: IWaitForChangedResult
	{
		private readonly WaitForChangedResult _instance;

		public WaitForChangedResultWrapper(WaitForChangedResult instance)
		{
			_instance = instance;
		}

		/// <inheritdoc cref="IWaitForChangedResult.ChangeType" />
		public WatcherChangeTypes ChangeType
			=> _instance.ChangeType;

		/// <inheritdoc cref="IWaitForChangedResult.Name" />
		public string? Name
			=> _instance.Name;

		/// <inheritdoc cref="IWaitForChangedResult.OldName" />
		public string? OldName
			=> _instance.OldName;

		/// <inheritdoc cref="IWaitForChangedResult.TimedOut" />
		public bool TimedOut
			=> _instance.TimedOut;
	}
}
