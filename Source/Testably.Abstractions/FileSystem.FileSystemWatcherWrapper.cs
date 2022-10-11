using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class FileSystemWatcherWrapper : IFileSystem.IFileSystemWatcher
	{
		private readonly FileSystemWatcher _instance;

		private FileSystemWatcherWrapper(FileSystemWatcher driveInfo,
		                                 IFileSystem fileSystem)
		{
			_instance = driveInfo;
			FileSystem = fileSystem;
		}

		#region IFileSystemWatcher Members

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
	}
}