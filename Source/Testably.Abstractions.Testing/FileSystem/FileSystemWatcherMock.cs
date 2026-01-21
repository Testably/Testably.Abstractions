using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
	private string _fullPath = string.Empty;

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
			FullPath = _path;
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

	/// <summary>
	/// Caches the full path of <see cref="Path"/>
	/// </summary>
	private string FullPath
	{
		get => _fullPath;
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				_fullPath = value;
				
				return;
			}
			
			string fullPath = GetNormalizedFullPath(value);
			
			_fullPath = fullPath;
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
		return MatchesWatcherPath(path, IncludeSubdirectories);
	}

	private bool MatchesWatcherPath(string? path, bool includeSubdirectories)
	{
		if (path == null)
		{
			return false;
		}

		string fullPath = _fileSystem.Execute.Path.GetFullPath(path);

		if (includeSubdirectories)
		{
			return fullPath.StartsWith(FullPath, _fileSystem.Execute.StringComparisonMode);
		}

		return string.Equals(
			GetNormalizedParent(path), FullPath, _fileSystem.Execute.StringComparisonMode
		);
	}

	private void NotifyChange(ChangeDescription item)
	{
		InternalEvent?.Invoke(this, new ChangeDescriptionEventArgs(item));
		if (MatchesFilter(item))
		{
			if (item.ChangeType.HasFlag(WatcherChangeTypes.Created))
			{
				Created?.Invoke(this, ToFileSystemEventArgs(
					item.ChangeType, item.Path));
			}

			if (item.ChangeType.HasFlag(WatcherChangeTypes.Deleted))
			{
				Deleted?.Invoke(this, ToFileSystemEventArgs(
					item.ChangeType, item.Path));
			}

			if (item.ChangeType.HasFlag(WatcherChangeTypes.Changed))
			{
				Changed?.Invoke(this, ToFileSystemEventArgs(
					item.ChangeType, item.Path));
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

	private void TriggerRenameNotification(ChangeDescription item)
	{
		// Outside: Outside the FullPath
		// Inside: FullPath/<target>
		// Nested: FullPath/*/<target>
		// Deep Nested: FullPath/*/**/<target>

		bool comesFromOutside = !MatchesWatcherPath(item.OldPath, true);
		bool goesToInside = MatchesWatcherPath(item.Path, false);

		// Outside -> Inside
		if (comesFromOutside && goesToInside)
		{
			Created?.Invoke(this, ToFileSystemEventArgs(WatcherChangeTypes.Created, item.Path));

			return;
		}

		bool comesFromInside = MatchesWatcherPath(item.OldPath, false);
		bool goesToOutside = !MatchesWatcherPath(item.Path, true);

		// ... -> Outside
		if (goesToOutside && (comesFromInside || IncludeSubdirectories))
		{
			Deleted?.Invoke(this, ToFileSystemEventArgs(WatcherChangeTypes.Deleted, item.OldPath!));

			return;
		}

		// Inside -> Inside
		if (comesFromInside && goesToInside)
		{
			if (TryMakeRenamedEventArgs(item, out RenamedEventArgs? eventArgs))
			{
				Renamed?.Invoke(this, eventArgs);
			}

			return;
		}

		RenamedContext context = new(
			comesFromOutside, comesFromInside, goesToInside, goesToOutside,
			GetSubDirectoryCount(item.OldPath!)
		);

		if (_fileSystem.Execute.IsWindows)
		{
			TriggerWindowsRenameNotification(item, context);
		}
		else if (_fileSystem.Execute.IsLinux)
		{
			TriggerLinuxRenameNotification(item, context);
		}
		else if (_fileSystem.Execute.IsMac)
		{
			TriggerMacRenameNotification(item, context);
		}
		else
		{
			if (TryMakeRenamedEventArgs(item, out RenamedEventArgs? eventArgs))
			{
				Renamed?.Invoke(this, eventArgs);
			}
		}
	}

	#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
	private void TriggerWindowsRenameNotification(ChangeDescription item, RenamedContext context)
	{
		CheckRenamePremise(context);

		if (context.ComesFromOutside)
		{
			if (IncludeSubdirectories)
			{
				FireCreated();
			}
		}
		else if (context.ComesFromInside)
		{
			FireDeleted();

			if (IncludeSubdirectories)
			{
				FireCreated();
			}
		}
		else if (context.ComesFromNested || context.ComesFromDeepNested)
		{
			if (context.GoesToInside)
			{
				if (IncludeSubdirectories)
				{
					FireDeleted();
				}

				FireCreated();
			}
			else if (IsItemNameChange(item) && IncludeSubdirectories)
			{
				FireRenamed();
			}
			else if (IncludeSubdirectories)
			{
				FireDeleted();
				FireCreated();
			}
		}

		return;

		void FireCreated()
		{
			Created?.Invoke(this, ToFileSystemEventArgs(WatcherChangeTypes.Created, item.Path));
		}

		void FireDeleted()
		{
			Deleted?.Invoke(this, ToFileSystemEventArgs(WatcherChangeTypes.Deleted, item.OldPath!));
		}

		void FireRenamed()
		{
			if (TryMakeRenamedEventArgs(item, out RenamedEventArgs? eventArgs))
			{
				Renamed?.Invoke(this, eventArgs);
			}
		}
	}

	private void TriggerMacRenameNotification(ChangeDescription item, RenamedContext context)
	{
		CheckRenamePremise(context);

		if (context.ComesFromInside && TryMakeRenamedEventArgs(item, out RenamedEventArgs? eventArgs))
		{
			Renamed?.Invoke(this, eventArgs);
			return;
		}

		TriggerLinuxRenameNotification(item, context);
	}

	private void TriggerLinuxRenameNotification(ChangeDescription item, RenamedContext context)
	{
		CheckRenamePremise(context);

		bool hasRenameArgs = TryMakeRenamedEventArgs(item, out RenamedEventArgs? eventArgs);

		if (context.ComesFromOutside)
		{
			if (IncludeSubdirectories)
			{
				Created?.Invoke(this, ToFileSystemEventArgs(WatcherChangeTypes.Created, item.Path));
			}
		}
		else if (context.ComesFromInside)
		{
			if (IncludeSubdirectories && hasRenameArgs)
			{
				Renamed?.Invoke(this, eventArgs!);
			}
			else
			{
				Deleted?.Invoke(
					this, ToFileSystemEventArgs(WatcherChangeTypes.Deleted, item.OldPath!)
				);
			}
		}
		else if (context.GoesToInside)
		{
			if (IncludeSubdirectories && hasRenameArgs)
			{
				Renamed?.Invoke(this, eventArgs!);
			}
			else
			{
				Created?.Invoke(this, ToFileSystemEventArgs(WatcherChangeTypes.Created, item.Path));
			}
		}
		else if (IncludeSubdirectories && hasRenameArgs)
		{
			Renamed?.Invoke(this, eventArgs!);
		}
	}
	#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high

	private static void CheckRenamePremise(RenamedContext context)
	{
		Debug.Assert(
			context is not { ComesFromOutside: true, GoesToInside: true },
			"The premise { ComesFromOutside: true, GoesToInside: true } should have been handled."
		);

		Debug.Assert(
			context is not { ComesFromInside: true, GoesToInside: true },
			"The premise { ComesFromInside: true, GoesToInside: true } should have been handled."
		);

		Debug.Assert(
			!context.GoesToOutside, "The premise { GoesToOutside: true } should have been handled."
		);
	}

	private string? GetNormalizedParent(string? path)
	{
		if (path == null)
		{
			return null;
		}

		string normalized = GetNormalizedFullPath(path);

		return _fileSystem.Execute.Path.GetDirectoryName(normalized)
			?.TrimEnd(_fileSystem.Execute.Path.DirectorySeparatorChar);
	}

	private string GetNormalizedFullPath(string path)
	{
		string normalized = _fileSystem.Execute.Path.GetFullPath(path);

		return normalized.TrimEnd(_fileSystem.Execute.Path.DirectorySeparatorChar);
	}

	/// <summary>
	/// Counts the number of directory separators inside the relative path to <see cref="FullPath"/>
	/// </summary>
	/// <param name="path"></param>
	/// <returns>The number of directory separators inside the relative path to <see cref="FullPath"/></returns>
	/// <remarks>Returns -1 if the path is outside the <see cref="FullPath"/></remarks>
	private int GetSubDirectoryCount(string path)
	{
		string normalizedPath = GetNormalizedFullPath(path);

		if (!normalizedPath.StartsWith(FullPath, _fileSystem.Execute.StringComparisonMode))
		{
			return -1;
		}

		return normalizedPath.Substring(FullPath.Length)
			.TrimStart(_fileSystem.Execute.Path.DirectorySeparatorChar)
			.Count(c => c == _fileSystem.Execute.Path.DirectorySeparatorChar);
	}

	private bool IsItemNameChange(ChangeDescription changeDescription)
	{
		string normalizedPath = GetNormalizedFullPath(changeDescription.Path);
		string normalizedOldPath = GetNormalizedFullPath(changeDescription.OldPath!);
		
		string name = _fileSystem.Execute.Path.GetFileName(normalizedPath);
		string oldName = _fileSystem.Execute.Path.GetFileName(normalizedOldPath);

		if (name.Equals(oldName, _fileSystem.Execute.StringComparisonMode))
		{
			return false;
		}

		if (name.Length == 0 || oldName.Length == 0)
		{
			return false;
		}
		
		string? parent = _fileSystem.Execute.Path.GetDirectoryName(normalizedPath);
		string? oldParent = _fileSystem.Execute.Path.GetDirectoryName(normalizedOldPath);
		
		return string.Equals(parent, oldParent, _fileSystem.Execute.StringComparisonMode);
	}

	private bool TryMakeRenamedEventArgs(
		ChangeDescription changeDescription,
		[NotNullWhen(true)] out RenamedEventArgs? eventArgs
	)
	{
		if (changeDescription.OldPath == null)
		{
			eventArgs = null;

			return false;
		}

		string name = TransformPathAndName(changeDescription.Path);

		string oldName = TransformPathAndName(changeDescription.OldPath);

		eventArgs = new RenamedEventArgs(changeDescription.ChangeType, Path, name, oldName);

		SetFileSystemEventArgsFullPath(eventArgs, name);
		SetRenamedEventArgsFullPath(eventArgs, oldName);

		return true;
	}

	private FileSystemEventArgs ToFileSystemEventArgs(
		WatcherChangeTypes changeType,
		string changePath)
	{
		string name = TransformPathAndName(changePath);

		FileSystemEventArgs eventArgs = new(changeType, Path, name);
		
		SetFileSystemEventArgsFullPath(eventArgs, name);

		return eventArgs;
	}

	private string TransformPathAndName(string changeDescriptionPath)
	{
		return changeDescriptionPath.Substring(FullPath.Length).TrimStart(_fileSystem.Execute.Path.DirectorySeparatorChar);
	}

	private void SetFileSystemEventArgsFullPath(FileSystemEventArgs args, string name)
	{
		if (_fileSystem.SimulationMode == SimulationMode.Native)
		{
			return;
		}
		
		string fullPath = _fileSystem.Execute.Path.Combine(Path, name);
		
		// FileSystemEventArgs implicitly combines the path in https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/System.IO.FileSystem.Watcher/src/System/IO/FileSystemEventArgs.cs
		// HACK: The combination uses the system separator, so to simulate the behavior, we must override it using reflection!
#if NETFRAMEWORK
			typeof(FileSystemEventArgs)
				.GetField("fullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
				.SetValue(args, fullPath);
#else
		typeof(FileSystemEventArgs)
			.GetField("_fullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
			.SetValue(args, fullPath);
#endif
	}

	private void SetRenamedEventArgsFullPath(RenamedEventArgs args, string oldName)
	{
		if (_fileSystem.SimulationMode == SimulationMode.Native)
		{
			return;
		}
		
		string fullPath = _fileSystem.Execute.Path.Combine(Path, oldName);
		
		// FileSystemEventArgs implicitly combines the path in https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/System.IO.FileSystem.Watcher/src/System/IO/FileSystemEventArgs.cs
		// HACK: The combination uses the system separator, so to simulate the behavior, we must override it using reflection!
#if NETFRAMEWORK
			typeof(RenamedEventArgs)
				.GetField("oldFullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
				.SetValue(args, fullPath);
#else
		typeof(RenamedEventArgs)
			.GetField("_oldFullPath", BindingFlags.Instance | BindingFlags.NonPublic)?
			.SetValue(args, fullPath);
#endif
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

	[StructLayout(LayoutKind.Auto)]
	private readonly struct RenamedContext(
		bool comesFromOutside,
		bool comesFromInside,
		bool goesToInside,
		bool goesToOutside,
		int oldSubDirectoryCount
	)
	{
		private const int NestedLevelCount = 1;

		public bool ComesFromOutside { get; } = comesFromOutside;

		public bool ComesFromInside { get; } = comesFromInside;

		public bool GoesToInside { get; } = goesToInside;

		public bool GoesToOutside { get; } = goesToOutside;

		/// <remarks>
		/// If this is <see langword="true"/> then <see cref="ComesFromDeepNested"/> is <see langword="false"/>
		/// </remarks>
		public bool ComesFromNested { get; } = oldSubDirectoryCount == NestedLevelCount;

		/// <remarks>
		/// If this is <see langword="true"/> then <see cref="ComesFromNested"/> is <see langword="false"/>
		/// </remarks>
		public bool ComesFromDeepNested { get; } = oldSubDirectoryCount > NestedLevelCount;
	}

	internal sealed class ChangeDescriptionEventArgs(ChangeDescription changeDescription)
		: EventArgs
	{
		public ChangeDescription ChangeDescription { get; } = changeDescription;
	}
}
