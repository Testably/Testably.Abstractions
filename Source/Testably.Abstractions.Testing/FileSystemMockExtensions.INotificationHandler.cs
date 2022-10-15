using System;
using System.IO;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public static partial class FileSystemMockExtensions
{
	/// <summary>
	///     Callback executed when a directory under <paramref name="path" /> matching the <paramref name="searchPattern" />
	///     was created.
	/// </summary>
	/// <param name="handler">The notification handler</param>
	/// <param name="notificationCallback">The callback to execute after the directory was created.</param>
	/// <param name="path">
	///     (optional) The root path in which to search for a created directory.<br />
	///     Defaults to the empty string, which matches all root directories.
	/// </param>
	/// <param name="searchPattern">
	///     (optional) The search pattern that the directory name must match.<br />
	///     Defaults to "*" which matches all directory names.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>An <see cref="Notification.IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static Notification.IAwaitableCallback<FileSystemMock.ChangeDescription>
		OnDirectoryCreated(
			this FileSystemMock.INotificationHandler handler,
			Action<FileSystemMock.ChangeDescription>? notificationCallback = null,
			string path = "",
			string searchPattern = "*",
			Func<FileSystemMock.ChangeDescription, bool>? predicate = null)
		=> handler.OnChange(notificationCallback,
			changeDescription => changeDescription.Matches(
				FileSystemTypes.Directory,
				WatcherChangeTypes.Created,
				handler.FileSystem.Path.GetFullPath(path),
				searchPattern,
				predicate));

	/// <summary>
	///     Callback executed when a directory under <paramref name="path" /> matching the <paramref name="searchPattern" />
	///     was deleted.
	/// </summary>
	/// <param name="handler">The notification handler</param>
	/// <param name="notificationCallback">The callback to execute after the directory was deleted.</param>
	/// <param name="path">
	///     (optional) The root path in which to search for a deleted directory.<br />
	///     Defaults to the empty string, which matches all root directories.
	/// </param>
	/// <param name="searchPattern">
	///     (optional) The search pattern that the directory name must match.<br />
	///     Defaults to "*" which matches all directory names.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>An <see cref="Notification.IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static Notification.IAwaitableCallback<FileSystemMock.ChangeDescription>
		OnDirectoryDeleted(
			this FileSystemMock.INotificationHandler handler,
			Action<FileSystemMock.ChangeDescription>? notificationCallback = null,
			string path = "",
			string searchPattern = "*",
			Func<FileSystemMock.ChangeDescription, bool>? predicate = null)
		=> handler.OnChange(notificationCallback,
			changeDescription => changeDescription.Matches(
				FileSystemTypes.Directory,
				WatcherChangeTypes.Deleted,
				handler.FileSystem.Path.GetFullPath(path),
				searchPattern,
				predicate));

	internal static bool Matches(this FileSystemMock.ChangeDescription changeDescription,
	                             FileSystemTypes fileSystemType,
	                             WatcherChangeTypes changeType,
	                             string path,
	                             string searchPattern,
	                             Func<FileSystemMock.ChangeDescription, bool>? predicate)
	{
		if (changeDescription.ChangeType != changeType ||
		    !changeDescription.FileSystemType.HasFlag(fileSystemType))
		{
			return false;
		}

		if (!string.IsNullOrEmpty(path) &&
		    !changeDescription.Path.StartsWith(path,
			    InMemoryLocation.StringComparisonMode))
		{
			return false;
		}

		if (searchPattern != "*" &&
		    (changeDescription.Name == null ||
		     !EnumerationOptionsHelper.MatchesPattern(EnumerationOptionsHelper.Compatible,
			     changeDescription.Name, searchPattern)))
		{
			return false;
		}

		return predicate?.Invoke(changeDescription) ?? true;
	}
}