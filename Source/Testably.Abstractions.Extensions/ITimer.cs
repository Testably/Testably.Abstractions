using System;
using System.Threading;

namespace Testably.Abstractions;

/// <summary>
///     A timer.
/// </summary>
public interface ITimer : IDisposable
{
    /// <summary>
    ///     The interval in which to execute the <see cref="Callback" />.
    /// </summary>
    TimeSpan Interval { get; }

    /// <summary>
    ///     The action to execute in each iteration.
    /// </summary>
    Action<CancellationToken> Callback { get; }

    /// <summary>
    ///     (optional) A callback for handling errors thrown by the <see cref="Callback" />.
    /// </summary>
    Action<Exception>? OnError { get; }
}