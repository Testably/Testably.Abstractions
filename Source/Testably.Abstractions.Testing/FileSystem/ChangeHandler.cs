using System;
using System.Collections.Generic;
using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class ChangeHandler
	: IInterceptionHandler, INotificationHandler, IWatcherTriggeredHandler
{
	private readonly Notification.INotificationFactory<ChangeDescription>
		_changeOccurredCallbacks = Notification.CreateFactory<ChangeDescription>();

	private readonly Notification.INotificationFactory<ChangeDescription>
		_changeOccurringCallbacks = Notification.CreateFactory<ChangeDescription>();

	private readonly List<ChangeDescription>? _history;
#if NET9_0_OR_GREATER
	private readonly System.Threading.Lock _historyLock = new();
#else
	private readonly object _historyLock = new();
#endif

	private readonly MockFileSystem _mockFileSystem;

	private readonly List<WatcherChangeDescription>? _watcherHistory;
#if NET9_0_OR_GREATER
	private readonly System.Threading.Lock _watcherHistoryLock = new();
#else
	private readonly object _watcherHistoryLock = new();
#endif

	private readonly Notification.INotificationFactory<WatcherChangeDescription>
		_watcherNotificationTriggeredCallbacks =
			Notification.CreateFactory<WatcherChangeDescription>();

	public ChangeHandler(MockFileSystem mockFileSystem, bool recordNotificationHistory)
	{
		_mockFileSystem = mockFileSystem;
		_history = recordNotificationHistory ? new List<ChangeDescription>() : null;
		_watcherHistory = recordNotificationHistory
			? new List<WatcherChangeDescription>()
			: null;
	}

	#region IInterceptionHandler Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem => _mockFileSystem;

	/// <inheritdoc cref="IInterceptionHandler.Event" />
	public IAwaitableCallback<ChangeDescription> Event(
		Action<ChangeDescription> interceptionCallback,
		Func<ChangeDescription, bool>? predicate = null)
		=> _changeOccurringCallbacks.RegisterCallback(interceptionCallback, predicate);

	#endregion

	#region INotificationHandler Members

	/// <inheritdoc cref="INotificationHandler.OnEvent" />
	public IAwaitableCallback<ChangeDescription> OnEvent(
		Action<ChangeDescription>? notificationCallback = null,
		Func<ChangeDescription, bool>? predicate = null)
		=> _changeOccurredCallbacks.RegisterCallback(notificationCallback, predicate);

	/// <inheritdoc cref="INotificationHandler.OnEventOrReplay" />
	public IAwaitableCallback<ChangeDescription> OnEventOrReplay(
		Action<ChangeDescription>? notificationCallback = null,
		Func<ChangeDescription, bool>? predicate = null)
	{
		if (_history is null)
		{
			throw new InvalidOperationException(
				$"{nameof(OnEventOrReplay)} requires notification history, but it was disabled via " +
				$"{nameof(MockFileSystem.MockFileSystemOptions)}." +
				$"{nameof(MockFileSystem.MockFileSystemOptions.WithoutNotificationHistory)}. " +
				$"Use {nameof(OnEvent)} instead, or remove the opt-out.");
		}

		IAwaitableCallback<ChangeDescription> waiter;
		ChangeDescription[] snapshot;
		lock (_historyLock)
		{
			waiter =
				_changeOccurredCallbacks.RegisterCallback(notificationCallback, predicate);
			snapshot = _history.ToArray();
		}

		try
		{
			foreach (ChangeDescription past in snapshot)
			{
				_changeOccurredCallbacks.Replay(waiter, past);
			}
		}
		catch
		{
			waiter.Dispose();
			throw;
		}

		return waiter;
	}

	#endregion

	#region IWatcherTriggeredHandler Members

	/// <inheritdoc cref="IWatcherTriggeredHandler.OnTriggered" />
	public IAwaitableCallback<WatcherChangeDescription> OnTriggered(
		Action<WatcherChangeDescription>? triggerCallback = null,
		Func<WatcherChangeDescription, bool>? predicate = null)
		=> _watcherNotificationTriggeredCallbacks.RegisterCallback(triggerCallback, predicate);

	/// <inheritdoc cref="IWatcherTriggeredHandler.OnTriggeredOrReplay" />
	public IAwaitableCallback<WatcherChangeDescription> OnTriggeredOrReplay(
		Action<WatcherChangeDescription>? triggerCallback = null,
		Func<WatcherChangeDescription, bool>? predicate = null)
	{
		if (_watcherHistory is null)
		{
			throw new InvalidOperationException(
				$"{nameof(OnTriggeredOrReplay)} requires notification history, but it was disabled via " +
				$"{nameof(MockFileSystem.MockFileSystemOptions)}." +
				$"{nameof(MockFileSystem.MockFileSystemOptions.WithoutNotificationHistory)}. " +
				$"Use {nameof(OnTriggered)} instead, or remove the opt-out.");
		}

		IAwaitableCallback<WatcherChangeDescription> waiter;
		WatcherChangeDescription[] snapshot;
		lock (_watcherHistoryLock)
		{
			waiter =
				_watcherNotificationTriggeredCallbacks.RegisterCallback(triggerCallback, predicate);
			snapshot = _watcherHistory.ToArray();
		}

		try
		{
			foreach (WatcherChangeDescription past in snapshot)
			{
				_watcherNotificationTriggeredCallbacks.Replay(waiter, past);
			}
		}
		catch
		{
			waiter.Dispose();
			throw;
		}

		return waiter;
	}

	#endregion

	internal void NotifyCompletedChange(ChangeDescription? fileSystemChange)
	{
		if (fileSystemChange is null)
		{
			return;
		}

		if (_history is null)
		{
			_changeOccurredCallbacks.InvokeCallbacks(fileSystemChange);
			return;
		}

		Action<ChangeDescription> invoke;
		lock (_historyLock)
		{
			_history.Add(fileSystemChange);
			invoke = _changeOccurredCallbacks.SnapshotInvocations();
		}

		invoke(fileSystemChange);
	}

	internal ChangeDescription NotifyPendingChange(WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		IStorageLocation location,
		IStorageLocation? oldLocation =
			null)
	{
		ChangeDescription fileSystemChange =
			new(changeType, fileSystemType, notifyFilters, location, oldLocation);
		_changeOccurringCallbacks.InvokeCallbacks(fileSystemChange);
		return fileSystemChange;
	}

	internal void NotifyWatcherTriggeredChange(ChangeDescription fileSystemChange,
		IFileSystemWatcher watcher)
	{
		WatcherChangeDescription watcherChange = new(fileSystemChange, watcher);

		if (_watcherHistory is null)
		{
			_watcherNotificationTriggeredCallbacks.InvokeCallbacks(watcherChange);
			return;
		}

		Action<WatcherChangeDescription> invoke;
		lock (_watcherHistoryLock)
		{
			_watcherHistory.Add(watcherChange);
			invoke = _watcherNotificationTriggeredCallbacks.SnapshotInvocations();
		}

		invoke(watcherChange);
	}
}
