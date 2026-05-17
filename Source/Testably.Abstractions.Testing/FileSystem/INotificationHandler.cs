using System;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     The notification handler for the <see cref="MockFileSystem" />.
/// </summary>
public interface INotificationHandler : IFileSystemEntity
{
	/// <summary>
	///     Callback executed when any change in the <see cref="MockFileSystem" /> matching the <paramref name="predicate" />
	///     occurred.
	/// </summary>
	/// <param name="notificationCallback">(optional) The callback to execute after the change occurred.</param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>An <see cref="IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	IAwaitableCallback<ChangeDescription> OnEvent(
		Action<ChangeDescription>? notificationCallback = null,
		Func<ChangeDescription, bool>? predicate = null);

	/// <summary>
	///     Like <see cref="OnEvent" />, but the returned callback also replays any matching changes
	///     that occurred before this call, in their original order.
	///     <para />
	///     Notification history is enabled by default. If it has been disabled via
	///     <see cref="MockFileSystem.MockFileSystemOptions.WithoutNotificationHistory" />, calling
	///     this method throws <see cref="InvalidOperationException" />; use <see cref="OnEvent" />
	///     instead in that case.
	/// </summary>
	/// <param name="notificationCallback">(optional) The callback to execute for each replayed and future change.</param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which changes are replayed and which future changes notify the callback.<br />
	///     If set to <see langword="null" /> (default value) all changes are considered.
	/// </param>
	/// <returns>An <see cref="IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	/// <exception cref="InvalidOperationException">
	///     Notification history was disabled via
	///     <see cref="MockFileSystem.MockFileSystemOptions.WithoutNotificationHistory" />.
	/// </exception>
	IAwaitableCallback<ChangeDescription> OnEventOrReplay(
		Action<ChangeDescription>? notificationCallback = null,
		Func<ChangeDescription, bool>? predicate = null);
}
