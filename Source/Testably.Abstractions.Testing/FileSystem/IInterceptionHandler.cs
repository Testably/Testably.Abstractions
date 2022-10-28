using System;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     The interception handler for the <see cref="MockFileSystem" />.
/// </summary>
public interface IInterceptionHandler : IFileSystemExtensionPoint
{
	/// <summary>
	///     Callback executed before any change in the <see cref="MockFileSystem" /> matching the <paramref name="predicate" />
	///     is about to occur.
	/// </summary>
	/// <param name="interceptionCallback">The callback to execute before the change occurred.</param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be intercepted.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are intercepted.
	/// </param>
	/// <remarks>This allows e.g. to throw custom exceptions instead.</remarks>
	MockFileSystem Event(Action<ChangeDescription> interceptionCallback,
						 Func<ChangeDescription, bool>? predicate = null);
}