using System;
using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class ChangeHandler : IInterceptionHandler,
	INotificationHandler
{
	private readonly Notification.INotificationFactory<ChangeDescription>
		_changeOccurredCallbacks = Notification.CreateFactory<ChangeDescription>();

	private readonly Notification.INotificationFactory<ChangeDescription>
		_changeOccurringCallbacks = Notification.CreateFactory<ChangeDescription>();

	private readonly MockFileSystem _mockFileSystem;

	public ChangeHandler(MockFileSystem mockFileSystem)
	{
		_mockFileSystem = mockFileSystem;
	}

	#region IInterceptionHandler Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem => _mockFileSystem;

	/// <inheritdoc
	///     cref="IInterceptionHandler.Event" />
	public MockFileSystem Event(
		Action<ChangeDescription> interceptionCallback,
		Func<ChangeDescription, bool>? predicate = null)
	{
		_changeOccurringCallbacks.RegisterCallback(interceptionCallback, predicate);
		return _mockFileSystem;
	}

	#endregion

	#region INotificationHandler Members

	/// <inheritdoc
	///     cref="INotificationHandler.OnEvent" />
	public Notification.IAwaitableCallback<ChangeDescription> OnEvent(
		Action<ChangeDescription>? notificationCallback = null,
		Func<ChangeDescription, bool>? predicate = null)
		=> _changeOccurredCallbacks.RegisterCallback(notificationCallback, predicate);

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
}
