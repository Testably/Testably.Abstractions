using System;
using System.Collections.Concurrent;

namespace Testably.Abstractions.Testing.Internal;

internal static class CallbackHandler
{
    public static void InvokeCallbacks<TCallbackValue>(
        ConcurrentDictionary<Guid, Action<TCallbackValue>> dictionary,
        TCallbackValue value)
    {
        foreach (Action<TCallbackValue> callback in dictionary.Values)
        {
            callback.Invoke(value);
        }
    }

    public static IDisposable RegisterCallback<TCallback>(
        ConcurrentDictionary<Guid, TCallback> dictionary,
        TCallback callback)
    {
        Guid key = Guid.NewGuid();
        dictionary.TryAdd(key, callback);

        return new RemoveCallback<TCallback>(dictionary, key);
    }

    private sealed class RemoveCallback<TCallback> : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, TCallback> _collection;
        private readonly Guid _key;

        public RemoveCallback(ConcurrentDictionary<Guid, TCallback> collection,
                              Guid key)
        {
            _collection = collection;
            _key = key;
        }

        #region IDisposable Members

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose()
        {
            _collection.TryRemove(_key, out _);
        }

        #endregion
    }
}