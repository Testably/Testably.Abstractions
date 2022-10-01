using System;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The notification handler for the <see cref="FileSystemMock" />.
    /// </summary>
    public interface INotificationHandler
    {
        /// <summary>
        ///     Callback executed when any change in the <see cref="FileSystemMock" /> matching the <paramref name="predicate" />
        ///     occurred.
        ///     <para />
        ///     Returns an <see cref="IDisposable" /> to un-register the callback.
        /// </summary>
        /// <param name="notificationCallback">The callback to execute after the change occurred.</param>
        /// <param name="predicate">
        ///     (optional) A predicate used to filter which callbacks should be notified.<br />
        ///     If set to <c>null</c> (default value) all callbacks are notified.
        /// </param>
        Notification.IAwaitableCallback<ChangeDescription> OnChange(
            Action<ChangeDescription>? notificationCallback = null,
            Func<ChangeDescription, bool>? predicate = null);
    }
}