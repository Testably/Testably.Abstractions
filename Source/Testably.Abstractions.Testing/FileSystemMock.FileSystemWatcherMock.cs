using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	/// <summary>
	///     Mocked instance of a <see cref="IFileSystem.IFileSystemWatcher" />
	/// </summary>
	public sealed class FileSystemWatcherMock : IFileSystem.IFileSystemWatcher
	{
		private CancellationTokenSource? _cancellationTokenSource;
		private IDisposable? _changeHandler;
		private readonly BlockingCollection<ChangeDescription> _changes = new(100);
		private bool _enableRaisingEvents;
		private readonly FileSystemMock _fileSystem;
		private readonly List<string> _filters = new();
		private string _path = string.Empty;

		private FileSystemWatcherMock(FileSystemMock fileSystem)
		{
			_fileSystem = fileSystem;
		}

		#region IFileSystemWatcher Members

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.EnableRaisingEvents" />
		public bool EnableRaisingEvents
		{
			get => _enableRaisingEvents;
			set
			{
				_enableRaisingEvents = value;
				if (_enableRaisingEvents)
				{
					Start();
				}
				else
				{
					Stop();
				}
			}
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem
			=> _fileSystem;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Filter" />
		public string Filter
		{
			get => _filters.Count == 0 ? "*" : _filters[0];
			set
			{
				_filters.Clear();
				_filters.Add(value);
			}
		}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Filters" />
		public ICollection<string> Filters
			=> _filters;
#endif

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IncludeSubdirectories" />
		public bool IncludeSubdirectories
		{
			get;
			set;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.InternalBufferSize" />
		public int InternalBufferSize
		{
			get;
			set;
		} = 8192;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.NotifyFilter" />
		public NotifyFilters NotifyFilter
		{
			get;
			set;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Path" />
		public string Path
		{
			get => _path;
			set
			{
				if (!_fileSystem.Directory.Exists(value))
				{
					throw ExceptionFactory.DirectoryNameDoesNotExist(value);
				}

				_path = value;
			}
		}

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
			=> Stop();

		#endregion

		internal static FileSystemWatcherMock New(FileSystemMock fileSystem)
			=> new(fileSystem);

		private bool MatchesFilter(ChangeDescription changeDescription)
		{
			if (_filters.Count == 0)
			{
				return true;
			}

			return _filters.Any(filter =>
				EnumerationOptionsHelper.MatchesPattern(
					EnumerationOptionsHelper.Compatible,
					_fileSystem.Path.GetFileName(changeDescription.Path),
					filter));
		}

		private void NotifyChange(ChangeDescription item)
		{
			if (MatchesFilter(item))
			{
				if (item.Type.HasFlag(ChangeTypes.Created))
				{
					Deleted?.Invoke(this,
						new FileSystemEventArgs(WatcherChangeTypes.Created,
							_fileSystem.Path.GetDirectoryName(item.Path) ?? "",
							_fileSystem.Path.GetFileName(item.Path)));
				}
				if (item.Type.HasFlag(ChangeTypes.Deleted))
				{
					Deleted?.Invoke(this,
						new FileSystemEventArgs(WatcherChangeTypes.Deleted,
							_fileSystem.Path.GetDirectoryName(item.Path) ?? "",
							_fileSystem.Path.GetFileName(item.Path)));
				}
				if (item.Type.HasFlag(ChangeTypes.Modified))
				{
					Deleted?.Invoke(this,
						new FileSystemEventArgs(WatcherChangeTypes.Changed,
							_fileSystem.Path.GetDirectoryName(item.Path) ?? "",
							_fileSystem.Path.GetFileName(item.Path)));
				}
				if (item.Type.HasFlag(ChangeTypes.Renamed))
				{
					Deleted?.Invoke(this,
						new FileSystemEventArgs(WatcherChangeTypes.Renamed,
							_fileSystem.Path.GetDirectoryName(item.Path) ?? "",
							_fileSystem.Path.GetFileName(item.Path)));
				}
			}
		}

		private void Start()
		{
			Stop();
			_cancellationTokenSource = new CancellationTokenSource();
			_changeHandler = _fileSystem.Notify.OnChange(c =>
			{
				if (!_changes.TryAdd(c, 100))
				{
					Error?.Invoke(this, new ErrorEventArgs(new TimeoutException("TODO")));
				}
			});
			CancellationToken token = _cancellationTokenSource.Token;
			Task.Factory.StartNew(() =>
			{
				while (!token.IsCancellationRequested)
				{
					if (_changes.TryTake(out ChangeDescription? c, Timeout.Infinite,
						token))
					{
						NotifyChange(c);
					}
				}
			}, TaskCreationOptions.LongRunning);
		}

		private void Stop()
		{
			_cancellationTokenSource?.Cancel();
			_changeHandler?.Dispose();
		}

#pragma warning disable CS0067 //TODO: Should be used and re-enabled
		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Changed" />
		public event FileSystemEventHandler? Changed;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Created" />
		public event FileSystemEventHandler? Created;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Deleted" />
		public event FileSystemEventHandler? Deleted;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Error" />
		public event ErrorEventHandler? Error;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Renamed" />
		public event RenamedEventHandler? Renamed;
#pragma warning restore CS0067
	}
}