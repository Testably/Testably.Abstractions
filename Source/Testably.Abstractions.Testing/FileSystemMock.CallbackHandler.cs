using System;
using System.IO;
using static Testably.Abstractions.Testing.FileSystemMock.ICallbackHandler;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    internal sealed class FileSystemMockCallbackHandler : ICallbackHandler
    {
        private readonly Notification.INotificationFactory<FileSystemChange>
            _changeOccurringCallbacks = Notification.CreateFactory<FileSystemChange>();

        private readonly Notification.INotificationFactory<FileSystemChange>
            _changeOccurredCallbacks = Notification.CreateFactory<FileSystemChange>();

        #region ICallbackHandler Members

        /// <inheritdoc cref="FileSystemMock.ICallbackHandler.ChangeOccurring(Action{ICallbackHandler.FileSystemChange})" />
        public ICallbackHandler ChangeOccurring(
            Action<FileSystemChange> callback)
        {
            _changeOccurringCallbacks.RegisterCallback(callback);
            return this;
        }

        /// <inheritdoc cref="FileSystemMock.ICallbackHandler.ChangeOccurred(Action{ICallbackHandler.FileSystemChange})" />
        public Notification.IAwaitableCallback<FileSystemChange> ChangeOccurred(
            Action<FileSystemChange>? callback = null)
            => _changeOccurredCallbacks.RegisterCallback(callback);

        #endregion

        public FileSystemChange InvokeChangeOccurring(string path,
                                                      ChangeType changeType,
                                                      NotifyFilters notifyFilters)
        {
            FileSystemChange fileSystemChange = new(path, changeType, notifyFilters);
            _changeOccurringCallbacks.InvokeCallbacks(fileSystemChange);
            return fileSystemChange;
        }

        public void InvokeChangeOccurred(FileSystemChange? fileSystemChange)
        {
            if (fileSystemChange != null)
            {
                _changeOccurredCallbacks.InvokeCallbacks(fileSystemChange);
            }
        }
    }
}