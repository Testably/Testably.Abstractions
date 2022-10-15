using System;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	/// <summary>
	///     The notification handler for the <see cref="FileSystemMock" />.
	/// </summary>
	public interface INotificationHandler : IFileSystem.IFileSystemExtensionPoint
	{
		/// <summary>
		///     Callback executed when any change in the <see cref="FileSystemMock" /> matching the <paramref name="predicate" />
		///     occurred.
		/// </summary>
		/// <param name="notificationCallback">The callback to execute after the change occurred.</param>
		/// <param name="predicate">
		///     (optional) A predicate used to filter which callbacks should be notified.<br />
		///     If set to <see langword="null" /> (default value) all callbacks are notified.
		/// </param>
		/// <returns>An <see cref="Notification.IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
		Notification.IAwaitableCallback<ChangeDescription> OnChange(
			Action<ChangeDescription>? notificationCallback = null,
			Func<ChangeDescription, bool>? predicate = null);
	}
}