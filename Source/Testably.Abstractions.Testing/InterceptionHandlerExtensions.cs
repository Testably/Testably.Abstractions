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
	///     Callback executed before a <paramref name="fileSystemType" /> matching the
	///     <paramref name="globPattern" /> is about to get changed.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute before the <paramref name="fileSystemType" /> is changed.</param>
	/// <param name="globPattern">
	///     (optional) The glob pattern that the <paramref name="fileSystemType" /> path must match.<br />
	///     Defaults to "**/*" which matches everything.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be intercepted.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are intercepted.
	/// </param>
	/// <returns>An <see cref="IAwaitableCallback{TValue}"/> for this event registration.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static IAwaitableCallback<ChangeDescription> Changing(
		this IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<ChangeDescription> interceptionCallback,
		string globPattern = "**/*",
		Func<ChangeDescription, bool>? predicate = null)
		=> handler.Event(interceptionCallback,
			changeDescription => changeDescription.Matches(
				handler.FileSystem.ExecuteOrDefault(),
				fileSystemType,
				WatcherChangeTypes.Changed,
				globPattern,
				predicate));

	/// <summary>
	///     Callback executed before a <paramref name="fileSystemType" /> matching the
	///     <paramref name="globPattern" /> is about to get created.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute before the <paramref name="fileSystemType" /> is created.</param>
	/// <param name="globPattern">
	///     (optional) The glob pattern that the <paramref name="fileSystemType" /> path must match.<br />
	///     Defaults to "**/*" which matches everything.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be intercepted.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are intercepted.
	/// </param>
	/// <returns>An <see cref="IAwaitableCallback{TValue}"/> for this event registration.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static IAwaitableCallback<ChangeDescription> Creating(
		this IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<ChangeDescription> interceptionCallback,
		string globPattern = "**/*",
		Func<ChangeDescription, bool>? predicate = null)
		=> handler.Event(interceptionCallback,
			changeDescription => changeDescription.Matches(
				handler.FileSystem.ExecuteOrDefault(),
				fileSystemType,
				WatcherChangeTypes.Created,
				globPattern,
				predicate));

	/// <summary>
	///     Callback executed before a <paramref name="fileSystemType" /> matching the
	///     <paramref name="globPattern" /> is about to get deleted.
	/// </summary>
	/// <param name="handler">The interception handler</param>
	/// <param name="fileSystemType">The type of the file system entry.</param>
	/// <param name="interceptionCallback">The callback to execute before the <paramref name="fileSystemType" /> is deleted.</param>
	/// <param name="globPattern">
	///     (optional) The glob pattern that the <paramref name="fileSystemType" /> path must match.<br />
	///     Defaults to "**/*" which matches everything.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be intercepted.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are intercepted.
	/// </param>
	/// <returns>An <see cref="IAwaitableCallback{TValue}"/> for this event registration.</returns>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	public static IAwaitableCallback<ChangeDescription> Deleting(
		this IInterceptionHandler handler,
		FileSystemTypes fileSystemType,
		Action<ChangeDescription> interceptionCallback,
		string globPattern = "**/*",
		Func<ChangeDescription, bool>? predicate = null)
		=> handler.Event(interceptionCallback,
			changeDescription => changeDescription.Matches(
				handler.FileSystem.ExecuteOrDefault(),
				fileSystemType,
				WatcherChangeTypes.Deleted,
				globPattern,
				predicate));
}
