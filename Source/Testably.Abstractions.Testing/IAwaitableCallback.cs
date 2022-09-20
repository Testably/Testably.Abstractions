using System;

namespace Testably.Abstractions.Testing;

/// <summary>
///     An object to allow<br />
///     - un-registering a callback by calling <see cref="IDisposable.Dispose()" /><br />
///     - blocking for the callback to be executed
/// </summary>
public interface IAwaitableCallback : IDisposable
{
    /// <summary>
    ///     Blocks the current thread until the callback is executed.
    ///     <para />
    ///     Returns <c>false</c> if the <paramref name="timeout" /> expired before the callback was executed, otherwise
    ///     <c>true</c>.
    /// </summary>
    /// <param name="timeout">
    ///     (optional) The timeout in milliseconds to wait on the callback.<br />
    ///     Defaults to <c>1000</c>ms.
    /// </param>
    bool Wait(int timeout = 1000);
}