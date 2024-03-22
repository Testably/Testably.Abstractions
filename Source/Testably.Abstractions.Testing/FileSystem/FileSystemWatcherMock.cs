using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Mocked instance of a <see cref="IFileSystemWatcher" />
/// </summary>
internal sealed class FileSystemWatcherMock : Component, IFileSystemWatcher
{
	/// <summary>
	///     Simulated bytes pre message to calculate the size of the blocking collection relative to the
	///     <see cref="InternalBufferSize" />.
	/// </summary>
	private const int BytesPerMessage = 128;

	private CancellationTokenSource? _cancellationTokenSource;
	private IDisposable? _changeHandler;
	private bool _enableRaisingEvents;
	private readonly MockFileSystem _fileSystem;
	private readonly Collection<string> _filters = new();
	private bool _includeSubdirectories;
	private int _internalBufferSize = 8192;
	private bool _isInitializing;

	private NotifyFilters _notifyFilter = NotifyFilters.FileName |
	                                      NotifyFilters.DirectoryName |
	                                      NotifyFilters.LastWrite;

	private string _path = string.Empty;

	private ISynchronizeInvoke? _synchronizingObject;

	private FileSystemWatcherMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	private event EventHandler<ChangeDescriptionEventArgs>? InternalEvent;

	#region IFileSystemWatcher Members

	/// <inheritdoc cref="IFileSystemWatcher.EnableRaisingEvents" />
	public bool EnableRaisingEvents
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(EnableRaisingEvents), PropertyAccess.Get);

			return _enableRaisingEvents;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(EnableRaisingEvents), PropertyAccess.Set);

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

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IFileSystemWatcher.Filter" />
	public string Filter
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(Filter), PropertyAccess.Get);

			if (_filters.Count == 0)
			{
				return _fileSystem.Execute.IsNetFramework ? "*.*" : "*";
			}

			return _filters[0];
		}
		set
		{
			using IDisposable registration = RegisterProperty(nameof(Filter), PropertyAccess.Set);

			_filters.Clear();
			_filters.Add(value);
		}
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	/// <inheritdoc cref="IFileSystemWatcher.Filters" />
	public Collection<string> Filters
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(Filters), PropertyAccess.Get);

			return _filters;
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemWatcher.IncludeSubdirectories" />
	public bool IncludeSubdirectories
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(IncludeSubdirectories), PropertyAccess.Get);

			return _includeSubdirectories;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(IncludeSubdirectories), PropertyAccess.Set);

			_includeSubdirectories = value;
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.InternalBufferSize" />
	public int InternalBufferSize
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(InternalBufferSize), PropertyAccess.Get);

			return _internalBufferSize;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(InternalBufferSize), PropertyAccess.Set);

			_internalBufferSize = Math.Max(value, 4096);
			Restart();
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.NotifyFilter" />
	public NotifyFilters NotifyFilter
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(NotifyFilter), PropertyAccess.Get);

			return _notifyFilter;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(NotifyFilter), PropertyAccess.Set);

			_notifyFilter = value;
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.Path" />
	public string Path
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(Path), PropertyAccess.Get);

			return _path;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(value, nameof(Path), PropertyAccess.Set);

			if (!string.IsNullOrEmpty(value) &&
			    !_fileSystem.Directory.Exists(value))
			{
				throw ExceptionFactory.DirectoryNameDoesNotExist(value, nameof(Path));
			}

			_path = value;
		}
	}

	/// <inheritdoc cref="Component.Site" />
	public override ISite? Site
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(Site), PropertyAccess.Get);

			return base.Site;
		}
		set
		{
			using IDisposable registration = RegisterProperty(nameof(Site), PropertyAccess.Set);

			base.Site = value;
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.SynchronizingObject" />
	public ISynchronizeInvoke? SynchronizingObject
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(SynchronizingObject), PropertyAccess.Get);

			return _synchronizingObject;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(SynchronizingObject), PropertyAccess.Set);

			_synchronizingObject = value;
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.BeginInit()" />
	public void BeginInit()
	{
		using IDisposable registration = RegisterMethod(nameof(BeginInit));

		_isInitializing = true;
		Stop();
	}

	/// <inheritdoc cref="IFileSystemWatcher.Changed" />
	public event FileSystemEventHandler? Changed;

	/// <inheritdoc cref="IFileSystemWatcher.Created" />
	public event FileSystemEventHandler? Created;

	/// <inheritdoc cref="IFileSystemWatcher.Deleted" />
	public event FileSystemEventHandler? Deleted;

	/// <inheritdoc cref="IFileSystemWatcher.EndInit()" />
	public void EndInit()
	{
		using IDisposable registration = RegisterMethod(nameof(EndInit));

		_isInitializing = false;
		Restart();
	}

	/// <inheritdoc cref="IFileSystemWatcher.Error" />
	[ExcludeFromCodeCoverage]
	public event ErrorEventHandler? Error;

	/// <inheritdoc cref="IFileSystemWatcher.Renamed" />
	public event RenamedEventHandler? Renamed;

	/// <inheritdoc cref="IFileSystemWatcher.WaitForChanged(WatcherChangeTypes)" />
	public IWaitForChangedResult WaitForChanged(
		WatcherChangeTypes changeType)
	{
		using IDisposable registration = RegisterMethod(nameof(WaitForChanged),
			changeType);

		return WaitForChanged(changeType, Timeout.Infinite);
	}

	/// <inheritdoc cref="IFileSystemWatcher.WaitForChanged(WatcherChangeTypes, int)" />
	public IWaitForChangedResult WaitForChanged(
		WatcherChangeTypes changeType, int timeout)
	{
		using IDisposable registration = RegisterMethod(nameof(WaitForChanged),
			changeType, timeout);

		return WaitForChangedInternal(changeType, TimeSpan.FromMilliseconds(timeout));
	}

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IFileSystemWatcher.WaitForChanged(WatcherChangeTypes, TimeSpan)" />
	public IWaitForChangedResult WaitForChanged(
		WatcherChangeTypes changeType, TimeSpan timeout)
	{
		using IDisposable registration = RegisterMethod(nameof(WaitForChanged),
			changeType, timeout);

		return WaitForChangedInternal(changeType, timeout);
	}
#endif

	#endregion

	internal static FileSystemWatcherMock New(MockFileSystem fileSystem)
		=> new(fileSystem);

	/// <inheritdoc cref="Component.Dispose(bool)" />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Stop();
		}

		base.Dispose(disposing);
	}

	private bool MatchesFilter(ChangeDescription changeDescription)
	{
		string fullPath = _fileSystem.Execute.Path.GetFullPath(Path);
		if (IncludeSubdirectories)
		{
			if (!changeDescription.Path.StartsWith(fullPath,
				_fileSystem.Execute.StringComparisonMode))
			{
				return false;
			}
		}
		else if (!string.Equals(_fileSystem.Execute.Path.GetDirectoryName(changeDescription.Path),
			fullPath))
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
				_fileSystem.Execute,
				EnumerationOptionsHelper.Compatible,
				_fileSystem.Execute.Path.GetFileName(changeDescription.Path),
				filter));
	}

	private void NotifyChange(ChangeDescription item)
	{
		InternalEvent?.Invoke(this, new ChangeDescriptionEventArgs(item));
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

	private IDisposable RegisterMethod(string name)
		=> _fileSystem.StatisticsRegistration.FileSystemWatcher.RegisterMethod(_path, name);

	private IDisposable RegisterMethod<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.FileSystemWatcher.RegisterMethod(_path, name,
			ParameterDescription.FromParameter(parameter1));

	private IDisposable RegisterMethod<T1, T2>(string name, T1 parameter1, T2 parameter2)
		=> _fileSystem.StatisticsRegistration.FileSystemWatcher.RegisterMethod(_path, name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2));

	private IDisposable RegisterProperty(string name, PropertyAccess access)
		=> _fileSystem.StatisticsRegistration.FileSystemWatcher.RegisterProperty(_path, name,
			access);

	private IDisposable RegisterProperty(string path, string name, PropertyAccess access)
		=> _fileSystem.StatisticsRegistration.FileSystemWatcher
			.RegisterProperty(path, name, access);

	private void Restart()
	{
		if (_isInitializing)
		{
			return;
		}

		if (EnableRaisingEvents)
		{
			Stop();
			Start();
		}
	}

	private void Start()
	{
		if (_isInitializing)
		{
			return;
		}

		Stop();
		CancellationTokenSource cancellationTokenSource = new();
		_cancellationTokenSource = cancellationTokenSource;

		int channelCapacity = InternalBufferSize / BytesPerMessage;
		Channel<ChangeDescription> channel =
			Channel.CreateBounded<ChangeDescription>(channelCapacity);
		ChannelWriter<ChangeDescription> writer = channel.Writer;
		ChannelReader<ChangeDescription> reader = channel.Reader;
		CancellationToken token = cancellationTokenSource.Token;
		_changeHandler = _fileSystem.Notify.OnEvent(c =>
		{
			if (!writer.TryWrite(c) &&
			    !token.IsCancellationRequested)
			{
				Error?.Invoke(this, new ErrorEventArgs(
					ExceptionFactory.InternalBufferOverflowException(
						InternalBufferSize, channelCapacity)));
			}
		});
		Task.Run(() =>
				{
					try
					{
						while (!token.IsCancellationRequested)
						{
							if (reader.TryRead(out ChangeDescription? c))
							{
								NotifyChange(c);
							}
						}
					}
					catch (Exception)
					{
						//Ignore any exception
					}
				},
				token)
			.ContinueWith(_ =>
			{
				if (channel.Writer.TryComplete())
				{
					channel.Reader.Completion.ContinueWith(_ =>
					{
						cancellationTokenSource.Dispose();
					}, CancellationToken.None);
				}
			}, TaskScheduler.Default);
	}

	private void Stop()
	{
		if (_cancellationTokenSource?.IsCancellationRequested == false)
		{
			_cancellationTokenSource.Cancel();
		}

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
		    _fileSystem.Execute.Path.IsPathRooted(changeDescriptionName))
		{
			transformedName = _fileSystem.Execute.Path.GetFileName(changeDescriptionPath);
			path = _fileSystem.Execute.Path.GetDirectoryName(path);
		}
		else if (path.EndsWith(transformedName, _fileSystem.Execute.StringComparisonMode))
		{
			path = path.Substring(0, path.Length - transformedName.Length);
		}

		name = transformedName;
		return path ?? "";
	}

	private void TriggerRenameNotification(ChangeDescription item)
		=> _fileSystem.Execute.OnWindows(
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
				_fileSystem.Execute.StringComparisonMode) ?? true;
	}

	private IWaitForChangedResult WaitForChangedInternal(
		WatcherChangeTypes changeType, TimeSpan timeout)
	{
		TaskCompletionSource<IWaitForChangedResult>
			tcs = new();

		void EventHandler(object? _, ChangeDescriptionEventArgs c)
		{
			if ((c.ChangeDescription.ChangeType & changeType) != 0)
			{
				tcs.TrySetResult(new WaitForChangedResultMock(
					c.ChangeDescription.ChangeType,
					c.ChangeDescription.Name,
					oldName: c.ChangeDescription.OldName,
					timedOut: false));
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

#if NETFRAMEWORK
		return tcs.Task.IsCompleted
			? tcs.Task.Result
			: WaitForChangedResultMock.TimedOutResult;
#else
		return tcs.Task.IsCompletedSuccessfully
			? tcs.Task.Result
			: WaitForChangedResultMock.TimedOutResult;
#endif
	}

	private struct WaitForChangedResultMock : IWaitForChangedResult
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

		/// <inheritdoc cref="IWaitForChangedResult.ChangeType" />
		public WatcherChangeTypes ChangeType { get; }

		/// <inheritdoc cref="IWaitForChangedResult.Name" />
		public string? Name { get; }

		/// <inheritdoc cref="IWaitForChangedResult.OldName" />
		public string? OldName { get; }

		/// <inheritdoc cref="IWaitForChangedResult.TimedOut" />
		public bool TimedOut { get; }
	}

	internal class ChangeDescriptionEventArgs(ChangeDescription changeDescription) : EventArgs
	{
		public ChangeDescription ChangeDescription { get; } = changeDescription;
	}
}
