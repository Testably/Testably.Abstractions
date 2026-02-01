using System;
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

	private readonly MockFileSystem _mockFileSystem;

	private readonly Notification.INotificationFactory<ChangeDescription>
		_watcherNotificationTriggeredCallbacks = Notification.CreateFactory<ChangeDescription>();

	public ChangeHandler(MockFileSystem mockFileSystem)
	{
		_mockFileSystem = mockFileSystem;
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

	#endregion

	#region IWatcherNotifiedHandler Members

	/// <inheritdoc cref="IWatcherTriggeredHandler.OnTriggered" />
	public IAwaitableCallback<ChangeDescription> OnTriggered(
		Action<ChangeDescription>? notificationCallback = null,
		Func<ChangeDescription, bool>? predicate = null)
		=> _watcherNotificationTriggeredCallbacks.RegisterCallback(notificationCallback, predicate);

	#endregion

	internal void NotifyCompletedChange(ChangeDescription? fileSystemChange)
	{
		if (fileSystemChange != null)
		{
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
