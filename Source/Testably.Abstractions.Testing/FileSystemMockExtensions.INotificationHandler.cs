using System;
using System.IO;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public static partial class FileSystemMockExtensions
{
	/// <summary>
	///     Callback executed when a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" />
	///     was changed.
	/// </summary>
	/// <param name="handler">The notification handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="notificationCallback">The callback to execute after the <paramref name="fileSystemType" /> was changed.</param>
	/// <param name="path">
	///     (optional) The root path in which to search for a changed <paramref name="fileSystemType" />.<br />
	///     Defaults to the empty string, which matches all root directories.
	/// </param>
	/// <param name="searchPattern">
	///     (optional) The search pattern that the <paramref name="fileSystemType" /> name must match.<br />
	///     Defaults to "*" which matches all <paramref name="fileSystemType" /> names.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>An <see cref="Notification.IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static Notification.IAwaitableCallback<FileSystemMock.ChangeDescription>
		OnChanged(
			this FileSystemMock.INotificationHandler handler,
			FileSystemTypes fileSystemType,
			Action<FileSystemMock.ChangeDescription>? notificationCallback = null,
			string path = "",
			string searchPattern = "*",
			Func<FileSystemMock.ChangeDescription, bool>? predicate = null)
		=> handler.OnEvent(notificationCallback,
			changeDescription => changeDescription.Matches(
				fileSystemType,
				WatcherChangeTypes.Changed,
				handler.FileSystem.Path.GetFullPath(path),
				searchPattern,
				predicate));

	/// <summary>
	///     Callback executed when a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" />
	///     was created.
	/// </summary>
	/// <param name="handler">The notification handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="notificationCallback">The callback to execute after the <paramref name="fileSystemType" /> was created.</param>
	/// <param name="path">
	///     (optional) The root path in which to search for a created <paramref name="fileSystemType" />.<br />
	///     Defaults to the empty string, which matches all root directories.
	/// </param>
	/// <param name="searchPattern">
	///     (optional) The search pattern that the <paramref name="fileSystemType" /> name must match.<br />
	///     Defaults to "*" which matches all <paramref name="fileSystemType" /> names.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>An <see cref="Notification.IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static Notification.IAwaitableCallback<FileSystemMock.ChangeDescription>
		OnCreated(
			this FileSystemMock.INotificationHandler handler,
			FileSystemTypes fileSystemType,
			Action<FileSystemMock.ChangeDescription>? notificationCallback = null,
			string path = "",
			string searchPattern = "*",
			Func<FileSystemMock.ChangeDescription, bool>? predicate = null)
		=> handler.OnEvent(notificationCallback,
			changeDescription => changeDescription.Matches(
				fileSystemType,
				WatcherChangeTypes.Created,
				handler.FileSystem.Path.GetFullPath(path),
				searchPattern,
				predicate));

	/// <summary>
	///     Callback executed when a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" />
	///     was deleted.
	/// </summary>
	/// <param name="handler">The notification handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="notificationCallback">The callback to execute after the <paramref name="fileSystemType" /> was deleted.</param>
	/// <param name="path">
	///     (optional) The root path in which to search for a deleted <paramref name="fileSystemType" />.<br />
	///     Defaults to the empty string, which matches all root directories.
	/// </param>
	/// <param name="searchPattern">
	///     (optional) The search pattern that the <paramref name="fileSystemType" /> name must match.<br />
	///     Defaults to "*" which matches all <paramref name="fileSystemType" /> names.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>An <see cref="Notification.IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static Notification.IAwaitableCallback<FileSystemMock.ChangeDescription>
		OnDeleted(
			this FileSystemMock.INotificationHandler handler,
			FileSystemTypes fileSystemType,
			Action<FileSystemMock.ChangeDescription>? notificationCallback = null,
			string path = "",
			string searchPattern = "*",
			Func<FileSystemMock.ChangeDescription, bool>? predicate = null)
		=> handler.OnEvent(notificationCallback,
			changeDescription => changeDescription.Matches(
				fileSystemType,
				WatcherChangeTypes.Deleted,
				handler.FileSystem.Path.GetFullPath(path),
				searchPattern,
				predicate));

	private static bool Matches(this FileSystemMock.ChangeDescription changeDescription,
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