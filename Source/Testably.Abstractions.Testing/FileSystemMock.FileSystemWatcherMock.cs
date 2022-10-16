using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	/// <summary>
	///     Mocked instance of a <see cref="IFileSystem.IFileSystemWatcher" />
	/// </summary>
	public sealed class FileSystemWatcherMock : Component, IFileSystem.IFileSystemWatcher
	{
		/// <summary>
		///     Simulated bytes pre message to calculate the size of the blocking collection relative to the
		///     <see cref="InternalBufferSize" />.
		/// </summary>
		private const int BytesPerMessage = 128;

		private static string DefaultFilter
			=> Execute.IsNetFramework ? "*.*" : "*";

		private CancellationTokenSource? _cancellationTokenSource;
		private IDisposable? _changeHandler;
		private BlockingCollection<ChangeDescription> _changes;
		private bool _enableRaisingEvents;
		private readonly FileSystemMock _fileSystem;
		private readonly List<string> _filters = new();
		private int _internalBufferSize = 8192;
		private string _path = string.Empty;
		private event EventHandler<ChangeDescription>? InternalEvent;

		private FileSystemWatcherMock(FileSystemMock fileSystem)
		{
			_fileSystem = fileSystem;
			_changes =
				new BlockingCollection<ChangeDescription>(InternalBufferSize /
				                                          BytesPerMessage);
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
				_internalBufferSize = Math.Max(value, 4096);
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

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.SynchronizingObject" />
		public ISynchronizeInvoke? SynchronizingObject { get; set; }

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.BeginInit()" />
		public void BeginInit()
			=> Stop();

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Changed" />
		public event FileSystemEventHandler? Changed;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Created" />
		public event FileSystemEventHandler? Created;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Deleted" />
		public event FileSystemEventHandler? Deleted;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.EndInit()" />
		public void EndInit()
			=> Restart();

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Error" />
		public event ErrorEventHandler? Error;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Renamed" />
		public event RenamedEventHandler? Renamed;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.WaitForChanged(WatcherChangeTypes)" />
		public IFileSystem.IFileSystemWatcher.IWaitForChangedResult WaitForChanged(
			WatcherChangeTypes changeType)
			=> WaitForChanged(changeType, Timeout.Infinite);

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.WaitForChanged(WatcherChangeTypes, int)" />
		public IFileSystem.IFileSystemWatcher.IWaitForChangedResult WaitForChanged(
			WatcherChangeTypes changeType,
			int timeout)
		{
			TaskCompletionSource<IFileSystem.IFileSystemWatcher.IWaitForChangedResult>
				tcs = new();

			void EventHandler(object? _, ChangeDescription c)
			{
				if ((c.ChangeType & changeType) != 0)
				{
					tcs.TrySetResult(new WaitForChangedResultMock(c.ChangeType, c.Name, oldName: c.OldName, timedOut: false));
				}
			}

			InternalEvent += EventHandler;
			try
			{
				bool wasEnabled = EnableRaisingEvents;
				if (!wasEnabled)
				{
					EnableRaisingEvents = true;
				}

				tcs.Task.Wait(timeout);
				EnableRaisingEvents = wasEnabled;
			}
			finally
			{
				InternalEvent -= EventHandler;
			}

			return tcs.Task.IsFaulted || tcs.Task.IsCanceled
				? WaitForChangedResultMock.TimedOutResult
				: tcs.Task.Result;
		}

		#endregion

		/// <inheritdoc cref="Component.Dispose(bool)" />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
			}

			base.Dispose(disposing);
		}

		internal static FileSystemWatcherMock New(FileSystemMock fileSystem)
			=> new(fileSystem);

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

			if ((NotifyFilter & changeDescription.NotifyFilters) == 0)
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
			InternalEvent?.Invoke(this, item);
			if (MatchesFilter(item))
			{
				if (item.ChangeType.HasFlag(WatcherChangeTypes.Created))
				{
					Created?.Invoke(this, ToFileSystemEventArgs(
						item.ChangeType, item.Path, item.Name));
				}

				if (item.ChangeType.HasFlag(WatcherChangeTypes.Deleted))
				{
					Deleted?.Invoke(this, ToFileSystemEventArgs(
						item.ChangeType, item.Path, item.Name));
				}

				if (item.ChangeType.HasFlag(WatcherChangeTypes.Changed))
				{
					Changed?.Invoke(this, ToFileSystemEventArgs(
						item.ChangeType, item.Path, item.Name));
				}

				if (item.ChangeType.HasFlag(WatcherChangeTypes.Renamed))
				{
					TriggerRenameNotification(item);
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
			_changeHandler = _fileSystem.Notify.OnEvent(c =>
			{
				if (!_changes.TryAdd(c, 100))
				{
					Error?.Invoke(this, new ErrorEventArgs(
						ExceptionFactory.InternalBufferOverflowException(
							InternalBufferSize, _changes.BoundedCapacity)));
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
			_cancellationTokenSource?.Cancel();
			_changeHandler?.Dispose();
		}

		private FileSystemEventArgs ToFileSystemEventArgs(
			WatcherChangeTypes changeType,
			string changePath,
			string? changeName)
		{
			string path = TransformPathAndName(
				changePath,
				changeName,
				out string name);

			return new FileSystemEventArgs(changeType,
				path, name);
		}

		private string TransformPathAndName(
			string changeDescriptionPath,
			string? changeDescriptionName,
			out string name)
		{
			string? transformedName = changeDescriptionName;
			string? path = changeDescriptionPath;
			if (transformedName == null ||
			    _fileSystem.Path.IsPathRooted(changeDescriptionName))
			{
				transformedName = _fileSystem.Path.GetFileName(changeDescriptionPath);
				path = _fileSystem.Path.GetDirectoryName(path);
			}
			else if (path.EndsWith(transformedName))
			{
				path = path.Substring(0, path.Length - transformedName.Length);
			}

			name = transformedName;
			return path ?? "";
		}

		private void TriggerRenameNotification(ChangeDescription item)
			=> Execute.OnWindows(
				() =>
				{
					if (TryMakeRenamedEventArgs(item,
						out RenamedEventArgs? eventArgs))
					{
						Renamed?.Invoke(this, eventArgs);
					}
					else if (item.OldPath != null)
					{
						Deleted?.Invoke(this, ToFileSystemEventArgs(
							item.ChangeType, item.OldPath, item.OldName));
						Created?.Invoke(this, ToFileSystemEventArgs(
							item.ChangeType, item.Path, item.Name));
					}
				},
				() =>
				{
					TryMakeRenamedEventArgs(item,
						out RenamedEventArgs? eventArgs);
					if (eventArgs != null)
					{
						Renamed?.Invoke(this, eventArgs);
					}
				});

		private bool TryMakeRenamedEventArgs(
			ChangeDescription changeDescription,
			[NotNullWhen(true)] out RenamedEventArgs? eventArgs)
		{
			if (changeDescription.OldPath == null)
			{
				eventArgs = null;
				return false;
			}

			string path = TransformPathAndName(
				changeDescription.Path,
				changeDescription.Name,
				out string name);

			TransformPathAndName(
				changeDescription.OldPath,
				changeDescription.OldName,
				out string oldName);

			eventArgs = new RenamedEventArgs(
				changeDescription.ChangeType,
				path,
				name,
				oldName);
			return System.IO.Path.GetDirectoryName(changeDescription.Path)?
			   .Equals(System.IO.Path.GetDirectoryName(changeDescription.OldPath),
					InMemoryLocation.StringComparisonMode) ?? true;
		}

		private struct WaitForChangedResultMock
			: IFileSystem.IFileSystemWatcher.IWaitForChangedResult
		{
			public WaitForChangedResultMock(
				WatcherChangeTypes changeType,
				string? name,
				string? oldName,
				bool timedOut)
			{
				ChangeType = changeType;
				Name = name;
				OldName = oldName;
				TimedOut = timedOut;
			}

			/// <summary>
			///     The instance representing a timed out <see cref="WaitForChangedResult" />.
			/// </summary>
			public static readonly WaitForChangedResultMock TimedOutResult =
				new(changeType: 0, name: null, oldName: null, timedOut: true);

			/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IWaitForChangedResult.ChangeType" />
			public WatcherChangeTypes ChangeType { get; }

			/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IWaitForChangedResult.Name" />
			public string? Name { get; }

			/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IWaitForChangedResult.OldName" />
			public string? OldName { get; }

			/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IWaitForChangedResult.TimedOut" />
			public bool TimedOut { get; }
		}
	}
}