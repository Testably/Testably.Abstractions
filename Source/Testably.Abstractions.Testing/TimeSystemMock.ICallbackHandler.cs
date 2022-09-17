using System;
using System.Threading;

namespace Testably.Abstractions.Testing;

public sealed partial class TimeSystemMock
{
    /// <summary>
    ///     The callback handler for the <see cref="TimeSystemMock" />
    /// </summary>
    public interface ICallbackHandler
    {
        /// <summary>
        ///     Callback executed when any of the following <c>DateTime</c> read methods is called:<br />
        ///     - <see cref="ITimeSystem.IDateTime.Now" /><br />
        ///     - <see cref="ITimeSystem.IDateTime.UtcNow" /><br />
        ///     - <see cref="ITimeSystem.IDateTime.Today" />
        ///     <para />
        ///     Returns an <see cref="IDisposable" /> to un-register the callback.
        /// </summary>
        IDisposable DateTimeRead(Action<DateTime> callback);

        /// <summary>
        ///     Callback executed when any of the following <c>Task.Delay</c> overloads is called:<br />
        ///     - <see cref="ITimeSystem.ITask.Delay(TimeSpan)" /><br />
        ///     - <see cref="ITimeSystem.ITask.Delay(TimeSpan, CancellationToken)" /><br />
        ///     - <see cref="ITimeSystem.ITask.Delay(int)" /><br />
        ///     - <see cref="ITimeSystem.ITask.Delay(int, CancellationToken)" />
        ///     <para />
        ///     Returns an <see cref="IDisposable" /> to un-register the callback.
        /// </summary>
        IDisposable TaskDelay(Action<TimeSpan> callback);

        /// <summary>
        ///     Callback executed when any of the following <c>Thread.Sleep</c> overloads is called:<br />
        ///     - <see cref="ITimeSystem.IThread.Sleep(TimeSpan)" /><br />
        ///     - <see cref="ITimeSystem.IThread.Sleep(int)" />
        ///     <para />
        ///     Returns an <see cref="IDisposable" /> to un-register the callback.
        /// </summary>
        IDisposable ThreadSleep(Action<TimeSpan> callback);
    }
}