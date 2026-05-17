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

	private readonly Notification.INotificationFactory<ChangeDescription>
		_watcherNotificationTriggeredCallbacks = Notification.CreateFactory<ChangeDescription>();

	public ChangeHandler(MockFileSystem mockFileSystem, bool recordNotificationHistory)
	{
		_mockFileSystem = mockFileSystem;
		_history = recordNotificationHistory ? new List<ChangeDescription>() : null;
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

		lock (_historyLock)
		{
			IAwaitableCallback<ChangeDescription> waiter =
				_changeOccurredCallbacks.RegisterCallback(notificationCallback, predicate);
			foreach (ChangeDescription past in _history)
			{
				_changeOccurredCallbacks.Replay(waiter, past);
			}

			return waiter;
		}
	}

	#endregion

	#region IWatcherTriggeredHandler Members

	/// <inheritdoc cref="IWatcherTriggeredHandler.OnTriggered" />
	public IAwaitableCallback<ChangeDescription> OnTriggered(
		Action<ChangeDescription>? triggerCallback = null,
		Func<ChangeDescription, bool>? predicate = null)
		=> _watcherNotificationTriggeredCallbacks.RegisterCallback(triggerCallback, predicate);

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

		lock (_historyLock)
		{
			_history.Add(fileSystemChange);
			_changeOccurredCallbacks.InvokeCallbacks(fileSystemChange);
		}
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

	internal void NotifyWatcherTriggeredChange(ChangeDescription fileSystemChange)
		=> _watcherNotificationTriggeredCallbacks.InvokeCallbacks(fileSystemChange);
}
