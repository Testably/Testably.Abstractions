using System;
using System.IO;
using static Testably.Abstractions.Testing.FileSystemMock.ICallbackHandler;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    internal sealed class FileSystemMockCallbackHandler : ICallbackHandler
    {
        private readonly Notification.INotificationFactory<CallbackChange>
            _changeOccurringCallbacks = Notification.CreateFactory<CallbackChange>();

        private readonly Notification.INotificationFactory<CallbackChange>
            _changeOccurredCallbacks = Notification.CreateFactory<CallbackChange>();

        #region ICallbackHandler Members

        /// <inheritdoc
        ///     cref="FileSystemMock.ICallbackHandler.ChangeOccurring(Action{ICallbackHandler.FileSystemChange}, Func{ICallbackHandler.FileSystemChange,bool})" />
        public ICallbackHandler ChangeOccurring(
            Action<CallbackChange> callback,
            Func<CallbackChange, bool>? predicate = null)
        {
            _changeOccurringCallbacks.RegisterCallback(callback, predicate);
            return this;
        }

        /// <inheritdoc
        ///     cref="FileSystemMock.ICallbackHandler.ChangeOccurred(Action{ICallbackHandler.FileSystemChange}, Func{ICallbackHandler.FileSystemChange,bool})" />
        public Notification.IAwaitableCallback<CallbackChange> ChangeOccurred(
            Action<CallbackChange>? callback = null,
            Func<CallbackChange, bool>? predicate = null)
            => _changeOccurredCallbacks.RegisterCallback(callback, predicate);

        #endregion

        public CallbackChange InvokeChangeOccurring(string path,
                                                      CallbackChangeTypes changeType,
                                                      NotifyFilters notifyFilters)
        {
            CallbackChange fileSystemChange = new(path, changeType, notifyFilters);
            _changeOccurringCallbacks.InvokeCallbacks(fileSystemChange);
            return fileSystemChange;
        }

        public void InvokeChangeOccurred(CallbackChange? fileSystemChange)
        {
            if (fileSystemChange != null)
            {
                _changeOccurredCallbacks.InvokeCallbacks(fileSystemChange);
            }
        }
    }
}