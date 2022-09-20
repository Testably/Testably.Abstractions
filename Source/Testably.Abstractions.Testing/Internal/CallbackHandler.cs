using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Testably.Abstractions.Testing.Internal;

internal static class CallbackHandler
{
    public static void InvokeCallbacks<TValue>(
        ConcurrentDictionary<Guid, CallbackWaiter<TValue>> dictionary,
        TValue value)
    {
        foreach (CallbackWaiter<TValue> callback in dictionary.Values)
        {
            callback.Invoke(value);
        }
    }

    public static IAwaitableCallback RegisterCallback<TValue>(
        ConcurrentDictionary<Guid, CallbackWaiter<TValue>> dictionary,
        Action<TValue>? callback)
    {
        Guid key = Guid.NewGuid();
        CallbackWaiter<TValue> callbackWaiter = new(dictionary, key, callback);
        dictionary.TryAdd(key, callbackWaiter);
        return callbackWaiter;
    }

    internal sealed class CallbackWaiter<TValue> : IAwaitableCallback
    {
        private readonly Action<TValue>? _callback;
        private readonly ConcurrentDictionary<Guid, CallbackWaiter<TValue>> _collection;
        private readonly Guid _key;
        private ManualResetEventSlim? _reset;

        public CallbackWaiter(
            ConcurrentDictionary<Guid, CallbackWaiter<TValue>> collection,
            Guid key, Action<TValue>? callback)
        {
            _collection = collection;
            _key = key;
            _callback = callback;
        }

        #region IAwaitableCallback Members

        /// <inheritdoc cref="IAwaitableCallback.Wait(int)" />
        public bool Wait(int timeout = 1000)
        {
            _reset ??= new ManualResetEventSlim();
            return _reset.Wait(timeout);
        }

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose()
        {
            _collection.TryRemove(_key, out _);
        }

        #endregion

        /// <summary>
        ///     Invokes the callback and resets the wait.
        /// </summary>
        /// <param name="value"></param>
        internal void Invoke(TValue value)
        {
            _callback?.Invoke(value);
            _reset?.Set();
        }
    }
}