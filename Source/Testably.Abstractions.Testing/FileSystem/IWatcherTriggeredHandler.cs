using System;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     The notification handler for triggered notifications from the <see cref="IFileSystemWatcher" />.
/// </summary>
public interface IWatcherTriggeredHandler : IFileSystemEntity
{
	/// <summary>
	///     Callback executed after the <see cref="IFileSystemWatcher" /> notified about a change in the
	///     <see cref="MockFileSystem" /> matching the <paramref name="predicate" />.
	/// </summary>
	/// <param name="triggerCallback">The callback to execute after the notification was triggered.</param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>An <see cref="IAwaitableCallback{WatcherChangeDescription}" /> to un-register the callback on dispose.</returns>
	IAwaitableCallback<WatcherChangeDescription> OnTriggered(
		Action<WatcherChangeDescription>? triggerCallback = null,
		Func<WatcherChangeDescription, bool>? predicate = null);

	/// <summary>
	///     Like <see cref="OnTriggered" />, but the returned callback also replays any matching
	///     watcher-triggered notifications that occurred before this call, in their original order.
	///     <para />
	///     Notification history is enabled by default. If it has been disabled via
	///     <see cref="MockFileSystem.MockFileSystemOptions.WithoutNotificationHistory" />, calling
	///     this method throws <see cref="InvalidOperationException" />; use <see cref="OnTriggered" />
	///     instead in that case.
	/// </summary>
	/// <param name="triggerCallback">(optional) The callback to execute for each replayed and future notification.</param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which notifications are replayed and which future notifications notify the callback.<br />
	///     If set to <see langword="null" /> (default value) all notifications are considered.
	/// </param>
	/// <returns>An <see cref="IAwaitableCallback{WatcherChangeDescription}" /> to un-register the callback on dispose.</returns>
	/// <exception cref="InvalidOperationException">
	///     Notification history was disabled via
	///     <see cref="MockFileSystem.MockFileSystemOptions.WithoutNotificationHistory" />.
	/// </exception>
	IAwaitableCallback<WatcherChangeDescription> OnTriggeredOrReplay(
		Action<WatcherChangeDescription>? triggerCallback = null,
		Func<WatcherChangeDescription, bool>? predicate = null);
}
