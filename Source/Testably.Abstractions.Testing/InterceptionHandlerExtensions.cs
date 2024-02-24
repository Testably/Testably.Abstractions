using System;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods for the <see cref="IInterceptionHandler" />
/// </summary>
public static class InterceptionHandlerExtensions
{
	/// <summary>
	///     Callback executed before a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" /> is about to get changed.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute before the <paramref name="fileSystemType" /> is changed.</param>
	/// <param name="path">
	///     (optional) The root path in which to search for a changed <paramref name="fileSystemType" />.<br />
	///     Defaults to the empty string, which matches all root directories.
	/// </param>
	/// <param name="searchPattern">
	///     (optional) The search pattern that the <paramref name="fileSystemType" /> name must match.<br />
	///     Defaults to "*" which matches all <paramref name="fileSystemType" /> names.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be intercepted.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are intercepted.
	/// </param>
	/// <returns>The <see cref="MockFileSystem" />.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static MockFileSystem Changing(
		this IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<ChangeDescription> interceptionCallback,
		string path = "",
		string searchPattern = "*",
		Func<ChangeDescription, bool>? predicate = null)
		=> handler.Event(interceptionCallback,
			changeDescription => changeDescription.Matches(
				(handler.FileSystem as MockFileSystem)?.Execute ?? Execute.Default,
				fileSystemType,
				WatcherChangeTypes.Changed,
				path.GetFullPathOrWhiteSpace(handler.FileSystem),
				searchPattern,
				predicate));

	/// <summary>
	///     Callback executed before a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" /> is about to get created.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute before the <paramref name="fileSystemType" /> is created.</param>
	/// <param name="path">
	///     (optional) The root path in which to search for a created <paramref name="fileSystemType" />.<br />
	///     Defaults to the empty string, which matches all root directories.
	/// </param>
	/// <param name="searchPattern">
	///     (optional) The search pattern that the <paramref name="fileSystemType" /> name must match.<br />
	///     Defaults to "*" which matches all <paramref name="fileSystemType" /> names.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be intercepted.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are intercepted.
	/// </param>
	/// <returns>The <see cref="MockFileSystem" />.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static MockFileSystem Creating(
		this IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<ChangeDescription> interceptionCallback,
		string path = "",
		string searchPattern = "*",
		Func<ChangeDescription, bool>? predicate = null)
		=> handler.Event(interceptionCallback,
			changeDescription => changeDescription.Matches(
				(handler.FileSystem as MockFileSystem)?.Execute ?? Execute.Default,
				fileSystemType,
				WatcherChangeTypes.Created,
				path.GetFullPathOrWhiteSpace(handler.FileSystem),
				searchPattern,
				predicate));

	/// <summary>
	///     Callback executed before a <paramref name="fileSystemType" /> under <paramref name="path" /> matching the
	///     <paramref name="searchPattern" /> is about to get deleted.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute before the <paramref name="fileSystemType" /> is deleted.</param>
	/// <param name="path">
	///     (optional) The root path in which to search for a deleted <paramref name="fileSystemType" />.<br />
	///     Defaults to the empty string, which matches all root directories.
	/// </param>
	/// <param name="searchPattern">
	///     (optional) The search pattern that the <paramref name="fileSystemType" /> name must match.<br />
	///     Defaults to "*" which matches all <paramref name="fileSystemType" /> names.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be intercepted.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are intercepted.
	/// </param>
	/// <returns>The <see cref="MockFileSystem" />.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static MockFileSystem Deleting(
		this IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<ChangeDescription> interceptionCallback,
		string path = "",
		string searchPattern = "*",
		Func<ChangeDescription, bool>? predicate = null)
		=> handler.Event(interceptionCallback,
			changeDescription => changeDescription.Matches(
				(handler.FileSystem as MockFileSystem)?.Execute ?? Execute.Default,
				fileSystemType,
				WatcherChangeTypes.Deleted,
				path.GetFullPathOrWhiteSpace(handler.FileSystem),
				searchPattern,
				predicate));
}
