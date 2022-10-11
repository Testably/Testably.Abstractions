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
		/// <summary>
		///     Simulated bytes pre message to calculate the size of the blocking collection relative to the
		///     <see cref="InternalBufferSize" />.
		/// </summary>
		private const int BytesPerMessage = 128;

		private static string DefaultFilter
			=> Framework.IsNetFramework ? "*.*" : "*";

		private CancellationTokenSource _cancellationTokenSource;
		private IDisposable? _changeHandler;
		private BlockingCollection<ChangeDescription> _changes;
		private bool _enableRaisingEvents;
		private readonly FileSystemMock _fileSystem;
		private readonly List<string> _filters = new();
		private int _internalBufferSize = 8192;
		private string _path = string.Empty;

		private FileSystemWatcherMock(FileSystemMock fileSystem)
		{
			_fileSystem = fileSystem;
			_changes =
				new BlockingCollection<ChangeDescription>(InternalBufferSize /
				                                          BytesPerMessage);
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationTokenSource.Cancel();
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
			get => _filters.Count == 0
				? DefaultFilter
				: _filters[0];
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
			get => _internalBufferSize;
			set
			{
				if (value < 4096)
				{
					value = 4096;
				}

				_internalBufferSize = value;
				Restart();
			}
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.NotifyFilter" />
		public NotifyFilters NotifyFilter
		{
			get;
			set;
		} = NotifyFilters.FileName |
		    NotifyFilters.DirectoryName |
		    NotifyFilters.LastWrite;

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

		private FileSystemEventArgs FromChangeDescription(
			ChangeDescription changeDescription, WatcherChangeTypes changeType)
		{
			string? name = changeDescription.Name;
			string? path = changeDescription.Path;
			if (name == null ||
			    _fileSystem.Path.IsPathRooted(changeDescription.Name))
			{
				name = _fileSystem.Path.GetFileName(changeDescription.Path);
				path = _fileSystem.Path.GetDirectoryName(path);
			}
			else if (path.EndsWith(name))
			{
				path = path.Substring(0, path.Length - name.Length);
			}

			return new FileSystemEventArgs(changeType, path ?? "", name);
		}

		private bool MatchesFilter(ChangeDescription changeDescription)
		{
			if (IncludeSubdirectories)
			{
				if (!changeDescription.Path.StartsWith(Path))
				{
					return false;
				}
			}
			else if (FileSystem.Path.GetDirectoryName(changeDescription.Path) != Path)
			{
				return false;
			}

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
					Created?.Invoke(this,
						FromChangeDescription(item, WatcherChangeTypes.Created));
				}

				if (item.Type.HasFlag(ChangeTypes.Deleted))
				{
					Deleted?.Invoke(this,
						FromChangeDescription(item, WatcherChangeTypes.Deleted));
				}

				if (item.Type.HasFlag(ChangeTypes.Modified))
				{
					Changed?.Invoke(this,
						FromChangeDescription(item, WatcherChangeTypes.Changed));
				}
			}
		}

		private void Restart()
		{
			if (EnableRaisingEvents)
			{
				Stop();
				BlockingCollection<ChangeDescription> changes = _changes;
				_changes =
					new BlockingCollection<ChangeDescription>(InternalBufferSize /
						BytesPerMessage);
				changes.Dispose();
				Start();
			}
			else
			{
				BlockingCollection<ChangeDescription> changes = _changes;
				_changes =
					new BlockingCollection<ChangeDescription>(InternalBufferSize /
						BytesPerMessage);
				changes.Dispose();
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
					Error?.Invoke(this, new ErrorEventArgs(
						new InternalBufferOverflowException(
							$"The internal buffer is greater than the {InternalBufferSize} allowed bytes (~ {_changes.BoundedCapacity} changes).")));
				}
			});
			CancellationToken token = _cancellationTokenSource.Token;
			Task.Factory.StartNew(() =>
			{
				try
				{
					while (!token.IsCancellationRequested)
					{
						if (_changes.TryTake(out ChangeDescription? c, Timeout.Infinite,
							token))
						{
							NotifyChange(c);
						}
					}
				}
				catch (Exception)
				{
					//Ignore any exception
				}
			}, TaskCreationOptions.LongRunning);
		}

		private void Stop()
		{
			_cancellationTokenSource.Cancel();
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