using System;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    internal sealed class FileSystemMockCallbackHandler : IInterceptionHandler,
        INotificationHandler
    {
        private readonly Notification.INotificationFactory<CallbackChange>
            _changeOccurringCallbacks = Notification.CreateFactory<CallbackChange>();

        private readonly Notification.INotificationFactory<CallbackChange>
            _changeOccurredCallbacks = Notification.CreateFactory<CallbackChange>();

        #region ICallbackHandler Members

        /// <inheritdoc
        ///     cref="IInterceptionHandler.Change(Action{CallbackChange}, Func{CallbackChange, bool}?)" />
        public INotificationHandler Change(
            Action<CallbackChange> callback,
            Func<CallbackChange, bool>? predicate = null)
        {
            _changeOccurringCallbacks.RegisterCallback(callback, predicate);
            return this;
        }

        /// <inheritdoc
        ///     cref="INotificationHandler.OnChange(Action{CallbackChange}?, Func{CallbackChange, bool}?)" />
        public Notification.IAwaitableCallback<CallbackChange> OnChange(
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