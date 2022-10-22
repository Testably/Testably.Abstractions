using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#if FEATURE_FILESYSTEMWATCHER_ADVANCED
using System.Collections.Generic;
#endif

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class FileSystemWatcherWrapper : IFileSystem.IFileSystemWatcher
	{
		private readonly FileSystemWatcher _instance;

		private FileSystemWatcherWrapper(FileSystemWatcher fileSystemWatcher,
		                                 IFileSystem fileSystem)
		{
			_instance = fileSystemWatcher;
			FileSystem = fileSystem;
		}

		#region IFileSystemWatcher Members

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Container" />
		public IContainer? Container
			=> _instance.Container;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.EnableRaisingEvents" />
		public bool EnableRaisingEvents
		{
			get => _instance.EnableRaisingEvents;
			set => _instance.EnableRaisingEvents = value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem { get; }

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Filter" />
		public string Filter
		{
			get => _instance.Filter;
			set => _instance.Filter = value;
		}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Filters" />
		public ICollection<string> Filters
			=> _instance.Filters;
#endif

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IncludeSubdirectories" />
		public bool IncludeSubdirectories
		{
			get => _instance.IncludeSubdirectories;
			set => _instance.IncludeSubdirectories = value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.InternalBufferSize" />
		public int InternalBufferSize
		{
			get => _instance.InternalBufferSize;
			set => _instance.InternalBufferSize = value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.NotifyFilter" />
		public NotifyFilters NotifyFilter
		{
			get => _instance.NotifyFilter;
			set => _instance.NotifyFilter = value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Path" />
		public string Path
		{
			get => _instance.Path;
			set => _instance.Path = value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Site" />
		public ISite? Site
		{
			get => _instance.Site;
			set => _instance.Site = value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.SynchronizingObject" />
		public ISynchronizeInvoke? SynchronizingObject
		{
			get => _instance.SynchronizingObject;
			set => _instance.SynchronizingObject = value;
		}

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
			=> _instance.Dispose();

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Changed" />
		public event FileSystemEventHandler? Changed
		{
			add => _instance.Changed += value;
			remove => _instance.Changed -= value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Created" />
		public event FileSystemEventHandler? Created
		{
			add => _instance.Created += value;
			remove => _instance.Created -= value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Deleted" />
		public event FileSystemEventHandler? Deleted
		{
			add => _instance.Deleted += value;
			remove => _instance.Deleted -= value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Error" />
		public event ErrorEventHandler? Error
		{
			add => _instance.Error += value;
			remove => _instance.Error -= value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Renamed" />
		public event RenamedEventHandler? Renamed
		{
			add => _instance.Renamed += value;
			remove => _instance.Renamed -= value;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.BeginInit()" />
		public void BeginInit()
			=> _instance.BeginInit();

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.EndInit()" />
		public void EndInit()
			=> _instance.EndInit();

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.WaitForChanged(WatcherChangeTypes)" />
		public IFileSystem.IFileSystemWatcher.IWaitForChangedResult WaitForChanged(
			WatcherChangeTypes changeType)
			=> new WaitForChangedResultWrapper(_instance.WaitForChanged(changeType));

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.WaitForChanged(WatcherChangeTypes, int)" />
		public IFileSystem.IFileSystemWatcher.IWaitForChangedResult WaitForChanged(
			WatcherChangeTypes changeType, int timeout)
			=> new WaitForChangedResultWrapper(
				_instance.WaitForChanged(changeType, timeout));

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
			: IFileSystem.IFileSystemWatcher.IWaitForChangedResult
		{
			private readonly WaitForChangedResult _instance;

			public WaitForChangedResultWrapper(WaitForChangedResult instance)
			{
				_instance = instance;
			}

			/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IWaitForChangedResult.ChangeType" />
			public WatcherChangeTypes ChangeType
				=> _instance.ChangeType;

			/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IWaitForChangedResult.Name" />
			public string? Name
				=> _instance.Name;

			/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IWaitForChangedResult.OldName" />
			public string? OldName
				=> _instance.OldName;

			/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IWaitForChangedResult.TimedOut" />
			public bool TimedOut
				=> _instance.TimedOut;
		}
	}
}