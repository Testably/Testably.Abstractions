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
	/// <returns>An <see cref="IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	IAwaitableCallback<ChangeDescription> OnTriggered(
		Action<ChangeDescription>? triggerCallback = null,
		Func<ChangeDescription, bool>? predicate = null);
}
