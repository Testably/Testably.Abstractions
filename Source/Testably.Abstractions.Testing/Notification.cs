using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Container for notification functionality.
/// </summary>
public static class Notification
{
    internal static INotificationFactory<TValue> CreateFactory<TValue>()
    {
        return new NotificationFactory<TValue>();
    }

    private sealed class NotificationFactory<TValue> : INotificationFactory<TValue>
    {
        private readonly ConcurrentDictionary<Guid, CallbackWaiter> _callbackWaiters =
            new();

        #region INotificationFactory<TValue> Members

        public void InvokeCallbacks(TValue value)
        {
            foreach (CallbackWaiter callback in _callbackWaiters.Values)
            {
                callback.Invoke(value);
            }
        }

        public IAwaitableCallback<TValue> RegisterCallback(
            Action<TValue>? callback)
        {
            Guid key = Guid.NewGuid();
            CallbackWaiter callbackWaiter = new(this, key, callback);
            _callbackWaiters.TryAdd(key, callbackWaiter);
            return callbackWaiter;
        }

        #endregion

        private void UnRegisterCallback(Guid key)
        {
            _callbackWaiters.TryRemove(key, out _);
        }

        private sealed class CallbackWaiter : IAwaitableCallback<TValue>
        {
            private readonly Action<TValue>? _callback;
            private int _count;
            private readonly NotificationFactory<TValue> _factory;
            private Func<TValue, bool>? _filter;
            private readonly Guid _key;
            private ManualResetEventSlim? _reset;

            public CallbackWaiter(
                NotificationFactory<TValue> factory,
                Guid key, Action<TValue>? callback)
            {
                _factory = factory;
                _key = key;
                _callback = callback;
            }

            #region IAwaitableCallback<TValue> Members

            /// <inheritdoc cref="IAwaitableCallback{TValue}.Wait(Func{TValue, bool}?, int, int)" />
            public bool Wait(Func<TValue, bool>? filter = null, int timeout = 1000,
                             int count = 1)
            {
                _count = count;
                _filter = filter;
                _reset ??= new ManualResetEventSlim();
                return _reset.Wait(timeout);
            }

            /// <inheritdoc cref="IDisposable.Dispose()" />
            public void Dispose()
            {
                _factory.UnRegisterCallback(_key);
            }

            #endregion

            /// <summary>
            ///     Invokes the callback and resets the wait.
            /// </summary>
            /// <param name="value"></param>
            internal void Invoke(TValue value)
            {
                _callback?.Invoke(value);
                if (_filter?.Invoke(value) != false)
                {
                    if (Interlocked.Decrement(ref _count) <= 0)
                    {
                        _reset?.Set();
                    }
                }
            }
        }
    }

    internal interface INotificationFactory<TValue>
    {
        void InvokeCallbacks(TValue value);

        IAwaitableCallback<TValue> RegisterCallback(
            Action<TValue>? callback);
    }

    /// <summary>
    ///     An object to allow<br />
    ///     - un-registering a callback by calling <see cref="IDisposable.Dispose()" /><br />
    ///     - blocking for the callback to be executed
    /// </summary>
    public interface IAwaitableCallback<out TValue> : IDisposable
    {
        /// <summary>
        ///     Blocks the current thread until the callback is executed.
        ///     <para />
        ///     Returns <c>false</c> if the <paramref name="timeout" /> expired before the callback was executed, otherwise
        ///     <c>true</c>.
        /// </summary>
        /// <param name="filter">
        ///     (optional) A filter for the callback to count.<br />
        ///     Defaults to <c>null</c> which will consider all callbacks.
        /// </param>
        /// <param name="timeout">
        ///     (optional) The timeout in milliseconds to wait on the callback.<br />
        ///     Defaults to <c>1000</c>ms.
        /// </param>
        /// <param name="count">
        ///     (optional) The number of callbacks to wait.<br />
        ///     Defaults to <c>1</c>
        /// </param>
        bool Wait(Func<TValue, bool>? filter = null, int timeout = 1000, int count = 1);
    }
}