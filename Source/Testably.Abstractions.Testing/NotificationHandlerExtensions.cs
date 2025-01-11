using System;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods for the <see cref="INotificationHandler" />
/// </summary>
public static class NotificationHandlerExtensions
{
	/// <summary>
	///     Callback executed when a <paramref name="fileSystemType" /> matching the
	///     <paramref name="globPattern" /> was changed.
	/// </summary>
	/// <param name="handler">The notification handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="notificationCallback">The callback to execute after the <paramref name="fileSystemType" /> was changed.</param>
	/// <param name="globPattern">
	///     (optional) The glob pattern that the <paramref name="fileSystemType" /> path must match.<br />
	///     Defaults to "**/*" which matches everything.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static IAwaitableCallback<ChangeDescription>
		OnChanged(
			this INotificationHandler handler,
			FileSystemTypes fileSystemType,
			Action<ChangeDescription>? notificationCallback = null,
			string globPattern = "**/*",
			Func<ChangeDescription, bool>? predicate = null)
		=> handler.OnEvent(notificationCallback,
			changeDescription => changeDescription.Matches(
				handler.FileSystem.ExecuteOrDefault(),
				fileSystemType,
				WatcherChangeTypes.Changed,
				globPattern,
				predicate));

	/// <summary>
	///     Callback executed when a <paramref name="fileSystemType" /> matching the
	///     <paramref name="globPattern" /> was created.
	/// </summary>
	/// <param name="handler">The notification handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="notificationCallback">The callback to execute after the <paramref name="fileSystemType" /> was created.</param>
	/// <param name="globPattern">
	///     (optional) The glob pattern that the <paramref name="fileSystemType" /> path must match.<br />
	///     Defaults to "**/*" which matches everything.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static IAwaitableCallback<ChangeDescription>
		OnCreated(
			this INotificationHandler handler,
			FileSystemTypes fileSystemType,
			Action<ChangeDescription>? notificationCallback = null,
			string globPattern = "**/*",
			Func<ChangeDescription, bool>? predicate = null)
		=> handler.OnEvent(notificationCallback,
			changeDescription => changeDescription.Matches(
				handler.FileSystem.ExecuteOrDefault(),
				fileSystemType,
				WatcherChangeTypes.Created,
				globPattern,
				predicate));

	/// <summary>
	///     Callback executed when a <paramref name="fileSystemType" /> matching the
	///     <paramref name="globPattern" /> was deleted.
	/// </summary>
	/// <param name="handler">The notification handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="notificationCallback">The callback to execute after the <paramref name="fileSystemType" /> was deleted.</param>
	/// <param name="globPattern">
	///     (optional) The glob pattern that the <paramref name="fileSystemType" /> path must match.<br />
	///     Defaults to "**/*" which matches everything.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static IAwaitableCallback<ChangeDescription>
		OnDeleted(
			this INotificationHandler handler,
			FileSystemTypes fileSystemType,
			Action<ChangeDescription>? notificationCallback = null,
			string globPattern = "**/*",
			Func<ChangeDescription, bool>? predicate = null)
		=> handler.OnEvent(notificationCallback,
			changeDescription => changeDescription.Matches(
				handler.FileSystem.ExecuteOrDefault(),
				fileSystemType,
				WatcherChangeTypes.Deleted,
				globPattern,
				predicate));
}
