using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Container for notification functionality.
/// </summary>
public static class Notification
{
	/// <summary>
	///     Executes the <paramref name="callback" /> while waiting for the notification.
	/// </summary>
	/// <returns>The <paramref name="awaitable" /> callback.</returns>
	public static IAwaitableCallback<TValue> ExecuteWhileWaiting<TValue>(
		this IAwaitableCallback<TValue> awaitable, Action callback)
	{
		return new CallbackWaiterWithValue<TValue, object?>(awaitable, () =>
		{
			callback();
			return null;
		});
	}

	/// <summary>
	///     Executes the <paramref name="callback" /> while waiting for the notification.
	/// </summary>
	/// <returns>The <paramref name="awaitable" /> callback.</returns>
	public static IAwaitableCallback<TValue, TFunc> ExecuteWhileWaiting<TValue, TFunc>(
		this IAwaitableCallback<TValue> awaitable, Func<TFunc> callback)
	{
		return new CallbackWaiterWithValue<TValue, TFunc>(awaitable, callback);
	}

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
			Action<TValue>? callback,
			Func<TValue, bool>? predicate = null)
		{
			Guid key = Guid.NewGuid();
			CallbackWaiter callbackWaiter = new(this, key, callback, predicate);
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
			private readonly Func<TValue, bool> _predicate;
			private readonly ManualResetEventSlim _reset;

			public CallbackWaiter(NotificationFactory<TValue> factory,
				Guid key, Action<TValue>? callback,
				Func<TValue, bool>? predicate)
			{
				_factory = factory;
				_key = key;
				_callback = callback;
				_predicate = predicate ?? (_ => true);
				_reset = new ManualResetEventSlim();
			}

			#region IAwaitableCallback<TValue> Members

			/// <inheritdoc cref="IAwaitableCallback{TValue}.Wait(Func{TValue, bool}?, int, int, Action?)" />
			public void Wait(Func<TValue, bool>? filter = null,
				int timeout = 30000,
				int count = 1,
				Action? executeWhenWaiting = null)
			{
				_count = count;
				_filter = filter;
				_reset.Reset();
				Task? task = null;
				if (executeWhenWaiting != null)
				{
					task = Task.Factory.StartNew(executeWhenWaiting.Invoke);
				}

				if (!_reset.Wait(timeout) ||
				    task?.Wait(timeout) == false)
				{
					throw ExceptionFactory.TimeoutExpired(timeout);
				}
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
				if (!_predicate.Invoke(value))
				{
					return;
				}

				_callback?.Invoke(value);
				if (_filter?.Invoke(value) != false &&
				    Interlocked.Decrement(ref _count) <= 0)
				{
					_reset.Set();
				}
			}
		}
	}

	internal interface INotificationFactory<TValue>
	{
		void InvokeCallbacks(TValue value);

		IAwaitableCallback<TValue> RegisterCallback(
			Action<TValue>? callback,
			Func<TValue, bool>? predicate = null);
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
		///     Throws a <see cref="TimeoutException" /> if the <paramref name="timeout" /> expired before the callback was
		///     executed.
		/// </summary>
		/// <param name="filter">
		///     (optional) A filter for the callback to count.<br />
		///     Defaults to <see langword="null" /> which will consider all callbacks.
		/// </param>
		/// <param name="timeout">
		///     (optional) The timeout in milliseconds to wait on the callback.<br />
		///     Defaults to <c>30000</c>ms (30 seconds).
		/// </param>
		/// <param name="count">
		///     (optional) The number of callbacks to wait.<br />
		///     Defaults to <c>1</c>
		/// </param>
		/// <param name="executeWhenWaiting">
		///     (optional) A callback to execute when waiting started.
		/// </param>
		void Wait(Func<TValue, bool>? filter = null,
			int timeout = 30000,
			int count = 1,
			Action? executeWhenWaiting = null);
	}

	/// <summary>
	///     An object to allow<br />
	///     - un-registering a callback by calling <see cref="IDisposable.Dispose()" /><br />
	///     - blocking for the callback to be executed
	/// </summary>
	public interface IAwaitableCallback<out TValue, out TFunc>
		: IAwaitableCallback<TValue>
	{
		/// <summary>
		///     Blocks the current thread until the callback is executed.
		///     <para />
		///     Throws a <see cref="TimeoutException" /> if the <paramref name="timeout" /> expired before the callback was
		///     executed.
		///     <para />
		///     Returns the value of <typeparamref name="TFunc" /> registered during an "Execute" call.
		/// </summary>
		/// <param name="filter">
		///     (optional) A filter for the callback to count.<br />
		///     Defaults to <see langword="null" /> which will consider all callbacks.
		/// </param>
		/// <param name="timeout">
		///     (optional) The timeout in milliseconds to wait on the callback.<br />
		///     Defaults to <c>30000</c>ms (30 seconds).
		/// </param>
		/// <param name="count">
		///     (optional) The number of callbacks to wait.<br />
		///     Defaults to <c>1</c>
		/// </param>
		/// <param name="executeWhenWaiting">
		///     (optional) A callback to execute when waiting started.
		/// </param>
		new TFunc Wait(Func<TValue, bool>? filter = null,
			int timeout = 30000,
			int count = 1,
			Action? executeWhenWaiting = null);
	}

	private sealed class CallbackWaiterWithValue<TValue, TFunc>
		: IAwaitableCallback<TValue, TFunc>
	{
		private readonly IAwaitableCallback<TValue> _awaitableCallback;
		private readonly Func<TFunc> _valueProvider;

		public CallbackWaiterWithValue(IAwaitableCallback<TValue> awaitableCallback,
			Func<TFunc> valueProvider)
		{
			_awaitableCallback = awaitableCallback;
			_valueProvider = valueProvider;
		}

		#region IAwaitableCallback<TValue,TFunc> Members

		/// <inheritdoc cref="IAwaitableCallback{TValue, TFunc}.Wait(Func{TValue, bool}?,int,int, Action?)" />
		public TFunc Wait(Func<TValue, bool>? filter = null,
			int timeout = 30000,
			int count = 1,
			Action? executeWhenWaiting = null)
		{
			TFunc value = default!;
			_awaitableCallback.Wait(filter, timeout, count, () =>
			{
				executeWhenWaiting?.Invoke();
				value = _valueProvider();
			});
			return value;
		}

		/// <inheritdoc cref="IAwaitableCallback{TValue}.Wait(Func{TValue, bool}?,int,int, Action?)" />
		void IAwaitableCallback<TValue>.Wait(Func<TValue, bool>? filter,
			int timeout,
			int count,
			Action? executeWhenWaiting)
		{
			_awaitableCallback.Wait(filter, timeout, count, () =>
			{
				executeWhenWaiting?.Invoke();
				_ = _valueProvider();
			});
		}

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
			=> _awaitableCallback.Dispose();

		#endregion
	}
}