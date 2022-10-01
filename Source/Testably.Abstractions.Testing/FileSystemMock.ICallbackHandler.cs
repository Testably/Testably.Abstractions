using System;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The callback handler for the <see cref="FileSystemMock" />
    /// </summary>
    public interface ICallbackHandler
    {
        /// <summary>
        ///     Callback executed when any change in the <see cref="FileSystemMock" /> is about to occur.
        ///     <para />
        ///     This allows e.g. to throw custom exceptions instead.
        /// </summary>
        ICallbackHandler ChangeOccurring(Action<CallbackChange> callback,
                                         Func<CallbackChange, bool>? predicate = null);

        /// <summary>
        ///     Callback executed when any change in the <see cref="FileSystemMock" /> occurred.
        ///     <para />
        ///     Returns an <see cref="IDisposable" /> to un-register the callback.
        /// </summary>
        Notification.IAwaitableCallback<CallbackChange> ChangeOccurred(
            Action<CallbackChange>? callback = null,
            Func<CallbackChange, bool>? predicate = null);
    }
}