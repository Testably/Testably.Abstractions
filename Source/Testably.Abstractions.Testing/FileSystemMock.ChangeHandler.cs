using System;
using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	internal sealed class ChangeHandlerImplementation : IInterceptionHandler,
		INotificationHandler
	{
		private readonly Notification.INotificationFactory<ChangeDescription>
			_changeOccurredCallbacks = Notification.CreateFactory<ChangeDescription>();

		private readonly Notification.INotificationFactory<ChangeDescription>
			_changeOccurringCallbacks = Notification.CreateFactory<ChangeDescription>();

		private readonly FileSystemMock _fileSystemMock;

		public ChangeHandlerImplementation(FileSystemMock fileSystemMock)
		{
			_fileSystemMock = fileSystemMock;
		}

		#region IInterceptionHandler Members

		/// <inheritdoc
		///     cref="IInterceptionHandler.Change(Action{ChangeDescription}, Func{ChangeDescription, bool}?)" />
		public FileSystemMock Change(
			Action<ChangeDescription> interceptionCallback,
			Func<ChangeDescription, bool>? predicate = null)
		{
			_changeOccurringCallbacks.RegisterCallback(interceptionCallback, predicate);
			return _fileSystemMock;
		}

		#endregion

		#region INotificationHandler Members

		/// <inheritdoc
		///     cref="INotificationHandler.OnChange(Action{ChangeDescription}?, Func{ChangeDescription, bool}?)" />
		public Notification.IAwaitableCallback<ChangeDescription> OnChange(
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

		internal ChangeDescription NotifyPendingChange(IStorageLocation location,
		                                               WatcherChangeTypes changeType,
		                                               FileSystemTypes fileSystemType,
		                                               NotifyFilters notifyFilters)
		{
			ChangeDescription fileSystemChange =
				new(location, changeType, fileSystemType, notifyFilters);
			_changeOccurringCallbacks.InvokeCallbacks(fileSystemChange);
			return fileSystemChange;
		}
	}
}