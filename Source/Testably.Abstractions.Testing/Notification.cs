using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
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
#if MarkExecuteWhileWaitingNotificationObsolete
	[Obsolete("Execute the callback before calling `Wait` or `WaitAsync` instead.")]
#endif
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
#if MarkExecuteWhileWaitingNotificationObsolete
	[Obsolete("Execute the callback before calling `Wait` or `WaitAsync` instead.")]
#endif
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
			private readonly Channel<TValue> _channel = Channel.CreateUnbounded<TValue>();
			private readonly NotificationFactory<TValue> _factory;
			private Func<TValue, bool>? _filter;
			private volatile bool _isDisposed;
			private readonly Guid _key;
			private readonly Func<TValue, bool> _predicate;
			private readonly ManualResetEventSlim _reset;
			private readonly ChannelWriter<TValue> _writer;

			public CallbackWaiter(NotificationFactory<TValue> factory,
				Guid key,
				Action<TValue>? callback,
				Func<TValue, bool>? predicate)
			{
				_factory = factory;
				_key = key;
				_callback = callback;
				_predicate = predicate ?? (_ => true);
				_reset = new ManualResetEventSlim();
				_writer = _channel.Writer;
			}

			#region IAwaitableCallback<TValue> Members

			/// <inheritdoc cref="IDisposable.Dispose()" />
			public void Dispose()
			{
				_factory.UnRegisterCallback(_key);
				_writer.TryComplete();
				_reset.Dispose();
				_isDisposed = true;
			}

			/// <inheritdoc cref="IAwaitableCallback{TValue}.Wait(Func{TValue, bool}?, int, int, Action?)" />
			public void Wait(Func<TValue, bool>? filter = null,
				int timeout = 30000,
				int count = 1,
				Action? executeWhenWaiting = null)
			{
				if (_isDisposed)
				{
					throw new ObjectDisposedException(
						"The awaitable callback is already disposed.");
				}

				_filter = filter;
				_reset.Reset();
				if (executeWhenWaiting != null)
				{
					executeWhenWaiting();
				}

				TValue[]? result = null;
				_ = Task.Run(async () =>
				{
					try
					{
						result = await WaitAsync(count, TimeSpan.FromMilliseconds(timeout));
					}
					catch
					{
						// Ignore exceptions as they will be handled by the timeout or cancellation token
					}
					finally
					{
						_reset.Set();
					}
				});

				if (!_reset.Wait(timeout) ||
				    result is null)
				{
					throw ExceptionFactory.TimeoutExpired(timeout);
				}
			}

			/// <inheritdoc />
			public TValue[] Wait(int count, TimeSpan? timeout = null)
			{
				if (_isDisposed)
				{
					throw new ObjectDisposedException(
						"The awaitable callback is already disposed.");
				}

				_reset.Reset();

				TValue[]? result = null;
				Task task = Task.Run(async () =>
				{
					try
					{
						result = await WaitAsync(count, timeout);
					}
					finally
					{
						_reset.Set();
					}
				});

				TimeSpan timeoutOrDefault = timeout ?? TimeSpan.FromSeconds(30);
				if (!_reset.Wait(timeoutOrDefault) ||
				    result is null)
				{
					throw task.Exception?.InnerException ??
					      ExceptionFactory.TimeoutExpired(timeoutOrDefault);
				}

				return result;
			}

			/// <inheritdoc cref="IAwaitableCallback{TValue}.WaitAsync(int, TimeSpan?, CancellationToken?)" />
			public async Task<TValue[]> WaitAsync(
				int count = 1,
				TimeSpan? timeout = null,
				CancellationToken? cancellationToken = null)
			{
				if (_isDisposed)
				{
					throw new ObjectDisposedException(
						"The awaitable callback is already disposed.");
				}

				List<TValue> values = [];
				ChannelReader<TValue> reader = _channel.Reader;

				CancellationTokenSource? cts = null;
				if (cancellationToken is null)
				{
					cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(30));
					cancellationToken = cts.Token;
				}
				else if (timeout is not null)
				{
					cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value);
					cts.CancelAfter(timeout.Value);
					cancellationToken = cts.Token;
				}

				try
				{
					do
					{
						TValue value = await reader.ReadAsync(cancellationToken.Value)
							.ConfigureAwait(false);
						if (_filter?.Invoke(value) != false)
						{
							values.Add(value);
							if (--count <= 0)
							{
								break;
							}
						}
					} while (!cancellationToken.Value.IsCancellationRequested);
				}
				finally
				{
					cts?.Dispose();
				}

				return values.ToArray();
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
				_writer.TryWrite(value);
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
#if MarkExecuteWhileWaitingNotificationObsolete
	[Obsolete("Will be removed when `ExecuteWhileWaiting` is removed.")]
#endif
	public interface IAwaitableCallback<TValue, out TFunc>
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

#if MarkExecuteWhileWaitingNotificationObsolete
	[Obsolete("Will be removed when `ExecuteWhileWaiting` is removed.")]
#endif
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

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
			=> _awaitableCallback.Dispose();

		/// <inheritdoc cref="IAwaitableCallback{TValue, TFunc}.Wait(Func{TValue, bool}?,int,int, Action?)" />
#if MarkExecuteWhileWaitingNotificationObsolete
		[Obsolete(
			"Use another `Wait` or `WaitAsync` overload and move the filter to the creation of the awaitable callback.")]
#endif
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

		/// <inheritdoc />
		public TValue[] Wait(int count, TimeSpan? timeout = null)
		{
			_valueProvider();
			return _awaitableCallback.Wait(count, timeout);
		}

		/// <inheritdoc />
		public Task<TValue[]> WaitAsync(int count = 1, TimeSpan? timeout = null,
			CancellationToken? cancellationToken = null)
		{
			_valueProvider();
			return _awaitableCallback.WaitAsync(count, timeout, cancellationToken);
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

		#endregion
	}
}
