using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
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
	private readonly Collection<string> _filters = [];
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
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(EnableRaisingEvents), PropertyAccess.Get);

			return _enableRaisingEvents;
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(EnableRaisingEvents), PropertyAccess.Set);

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
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(Filter), PropertyAccess.Get);

			if (_filters.Count == 0)
			{
				return _fileSystem.Execute.IsNetFramework ? "*.*" : "*";
			}

			return _filters[0];
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(Filter), PropertyAccess.Set);

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
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(Filters), PropertyAccess.Get);

			return _filters;
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemWatcher.IncludeSubdirectories" />
	public bool IncludeSubdirectories
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(IncludeSubdirectories), PropertyAccess.Get);

			return _includeSubdirectories;
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(IncludeSubdirectories), PropertyAccess.Set);

			_includeSubdirectories = value;
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.InternalBufferSize" />
	public int InternalBufferSize
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(InternalBufferSize), PropertyAccess.Get);

			return _internalBufferSize;
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(InternalBufferSize), PropertyAccess.Set);

			_internalBufferSize = Math.Max(value, 4096);
			Restart();
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.NotifyFilter" />
	public NotifyFilters NotifyFilter
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(NotifyFilter), PropertyAccess.Get);

			return _notifyFilter;
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(NotifyFilter), PropertyAccess.Set);

			_notifyFilter = value;
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.Path" />
	public string Path
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(Path), PropertyAccess.Get);

			return _path;
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(value,
					nameof(Path), PropertyAccess.Set);

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
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(Site), PropertyAccess.Get);

			return base.Site;
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(Site), PropertyAccess.Set);

			base.Site = value;
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.SynchronizingObject" />
	public ISynchronizeInvoke? SynchronizingObject
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(SynchronizingObject), PropertyAccess.Get);

			return _synchronizingObject;
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileSystemWatcher.RegisterPathProperty(_path,
					nameof(SynchronizingObject), PropertyAccess.Set);

			_synchronizingObject = value;
		}
	}

	/// <inheritdoc cref="IFileSystemWatcher.BeginInit()" />
	public void BeginInit()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileSystemWatcher.RegisterPathMethod(_path, nameof(BeginInit));

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
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileSystemWatcher.RegisterPathMethod(_path, nameof(EndInit));

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
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileSystemWatcher.RegisterPathMethod(_path, nameof(WaitForChanged),
				changeType);

		return WaitForChanged(changeType, Timeout.Infinite);
	}

	/// <inheritdoc cref="IFileSystemWatcher.WaitForChanged(WatcherChangeTypes, int)" />
	public IWaitForChangedResult WaitForChanged(
		WatcherChangeTypes changeType, int timeout)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileSystemWatcher.RegisterPathMethod(_path, nameof(WaitForChanged),
				changeType, timeout);

		return WaitForChangedInternal(changeType, TimeSpan.FromMilliseconds(timeout));
	}

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
	/// <inheritdoc cref="IFileSystemWatcher.WaitForChanged(WatcherChangeTypes, TimeSpan)" />
	public IWaitForChangedResult WaitForChanged(
		WatcherChangeTypes changeType, TimeSpan timeout)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileSystemWatcher.RegisterPathMethod(_path, nameof(WaitForChanged),
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
		if (!MatchesWatcherPath(changeDescription.Path))
		{
			if (changeDescription.ChangeType != WatcherChangeTypes.Renamed ||
			    !MatchesWatcherPath(changeDescription.OldPath))
			{
				return false;
			}
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

	private bool MatchesWatcherPath(string? path)
	{
		if (path == null)
		{
			return false;
		}

		string fullPath = _fileSystem.Execute.Path.GetFullPath(Path);
		if (IncludeSubdirectories)
		{
			return path.StartsWith(fullPath, _fileSystem.Execute.StringComparisonMode);
		}

		return string.Equals(_fileSystem.Execute.Path.GetDirectoryName(path), fullPath,
			_fileSystem.Execute.StringComparisonMode);
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
		_ = Task.Run(() =>
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
			.ContinueWith(__ =>
			{
				if (channel.Writer.TryComplete())
				{
					_ = channel.Reader.Completion.ContinueWith(_ =>
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

		FileSystemEventArgs eventArgs = new(changeType, Path, name);
		if (_fileSystem.SimulationMode != SimulationMode.Native)
		{
			// FileSystemEventArgs implicitly combines the path in https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/System.IO.FileSystem.Watcher/src/System/IO/FileSystemEventArgs.cs
			// HACK: Have to resort to Reflection to override this behavior!
#if NETFRAMEWORK
			typeof(FileSystemEventArgs)
				.GetField("fullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
				.SetValue(eventArgs, path);
#else
			typeof(FileSystemEventArgs)
				.GetField("_fullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
				.SetValue(eventArgs, path);
#endif
		}

		return eventArgs;
	}

	private string TransformPathAndName(
		string changeDescriptionPath,
		string? changeDescriptionName,
		out string name)
	{
		string? transformedName = changeDescriptionName;
		string? path = changeDescriptionPath;
		if (!_fileSystem.Path.IsPathRooted(Path))
		{
			string rootedWatchedPath = _fileSystem.Directory.GetCurrentDirectory();
			if (!rootedWatchedPath.EndsWith(_fileSystem.Path.DirectorySeparatorChar))
			{
				rootedWatchedPath += _fileSystem.Path.DirectorySeparatorChar;
			}

			if (path.StartsWith(rootedWatchedPath, _fileSystem.Execute.StringComparisonMode))
			{
				path = path.Substring(rootedWatchedPath.Length);
			}

			transformedName = _fileSystem.Execute.Path.GetFileName(changeDescriptionPath);
		}
		else if (transformedName == null ||
		         _fileSystem.Execute.Path.IsPathRooted(changeDescriptionName))
		{
			transformedName = _fileSystem.Execute.Path.GetFileName(changeDescriptionPath);
		}

		name = transformedName;

		return path ?? "";
	}

	private void TriggerRenameNotification(ChangeDescription item)
	{
		if (_fileSystem.Execute.IsWindows)
		{
			if (TryMakeRenamedEventArgs(item,
				out RenamedEventArgs? eventArgs))
			{
				Renamed?.Invoke(this, eventArgs);
			}
			else if (item.OldPath != null)
			{
				if (MatchesWatcherPath(item.OldPath))
				{
					Deleted?.Invoke(this, ToFileSystemEventArgs(
						WatcherChangeTypes.Deleted, item.OldPath, item.OldName));
				}

				if (MatchesWatcherPath(item.Path))
				{
					Created?.Invoke(this, ToFileSystemEventArgs(
						WatcherChangeTypes.Created, item.Path, item.Name));
				}
			}
		}
		else
		{
			TryMakeRenamedEventArgs(item,
				out RenamedEventArgs? eventArgs);
			if (eventArgs != null)
			{
				Renamed?.Invoke(this, eventArgs);
			}
		}
	}

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

		string oldPath = TransformPathAndName(
			changeDescription.OldPath,
			changeDescription.OldName,
			out string oldName);

		eventArgs = new RenamedEventArgs(
			changeDescription.ChangeType,
			Path,
			name,
			oldName);

		if (_fileSystem.SimulationMode != SimulationMode.Native)
		{
			// RenamedEventArgs implicitly combines the path in https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/System.IO.FileSystem.Watcher/src/System/IO/RenamedEventArgs.cs
			// HACK: Have to resort to Reflection to override this behavior!
#if NETFRAMEWORK
			typeof(FileSystemEventArgs)
				.GetField("fullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
				.SetValue(eventArgs, path);
			typeof(RenamedEventArgs)
				.GetField("oldFullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
				.SetValue(eventArgs, oldPath);
#else
			typeof(FileSystemEventArgs)
				.GetField("_fullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
				.SetValue(eventArgs, path);
			typeof(RenamedEventArgs)
				.GetField("_oldFullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
				.SetValue(eventArgs, oldPath);
#endif
		}

		return _fileSystem.Execute.Path.GetDirectoryName(changeDescription.Path)?
			.Equals(_fileSystem.Execute.Path.GetDirectoryName(changeDescription.OldPath),
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

			#pragma warning disable MA0040
			tcs.Task.Wait(timeout);
			#pragma warning restore MA0040
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

	internal sealed class ChangeDescriptionEventArgs(ChangeDescription changeDescription)
		: EventArgs
	{
		public ChangeDescription ChangeDescription { get; } = changeDescription;
	}
}
