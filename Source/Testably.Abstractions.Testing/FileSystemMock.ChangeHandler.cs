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

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem => _fileSystemMock;

		/// <inheritdoc
		///     cref="IInterceptionHandler.Changing" />
		public FileSystemMock Changing(
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
}