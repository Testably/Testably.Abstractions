﻿using System;
using System.IO;

namespace Testably.Abstractions.Testing;

public static partial class FileSystemMockExtensions
{
	/// <summary>
	///     Callback executed before a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" /> is about to get changed.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute after the <paramref name="fileSystemType" /> was changed.</param>
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
	/// <returns>The <see cref="FileSystemMock" />.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static FileSystemMock Changing(
		this FileSystemMock.IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<FileSystemMock.ChangeDescription> interceptionCallback,
		string path = "",
		string searchPattern = "*",
		Func<FileSystemMock.ChangeDescription, bool>? predicate = null)
		=> handler.Changing(interceptionCallback,
			changeDescription => changeDescription.Matches(
				fileSystemType,
				WatcherChangeTypes.Changed,
				handler.FileSystem.Path.GetFullPath(path),
				searchPattern,
				predicate));

	/// <summary>
	///     Callback executed before a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" /> is about to get created.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute after the <paramref name="fileSystemType" /> was created.</param>
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
	/// <returns>The <see cref="FileSystemMock" />.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static FileSystemMock Creating(
		this FileSystemMock.IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<FileSystemMock.ChangeDescription> interceptionCallback,
		string path = "",
		string searchPattern = "*",
		Func<FileSystemMock.ChangeDescription, bool>? predicate = null)
		=> handler.Changing(interceptionCallback,
			changeDescription => changeDescription.Matches(
				fileSystemType,
				WatcherChangeTypes.Created,
				handler.FileSystem.Path.GetFullPath(path),
				searchPattern,
				predicate));

	/// <summary>
	///     Callback executed before a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" /> is about to get deleted.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute after the <paramref name="fileSystemType" /> was deleted.</param>
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
	/// <returns>The <see cref="FileSystemMock" />.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static FileSystemMock Deleting(
		this FileSystemMock.IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<FileSystemMock.ChangeDescription> interceptionCallback,
		string path = "",
		string searchPattern = "*",
		Func<FileSystemMock.ChangeDescription, bool>? predicate = null)
		=> handler.Changing(interceptionCallback,
			changeDescription => changeDescription.Matches(
				fileSystemType,
				WatcherChangeTypes.Deleted,
				handler.FileSystem.Path.GetFullPath(path),
				searchPattern,
				predicate));
}