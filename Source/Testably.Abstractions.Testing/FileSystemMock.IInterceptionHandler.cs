using System;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The interception handler for the <see cref="FileSystemMock" />.
    /// </summary>
    public interface IInterceptionHandler
    {
        /// <summary>
        ///     Callback executed when any change in the <see cref="FileSystemMock" /> matching the <paramref name="predicate" />
        ///     is about to occur.
        ///     <para />
        ///     This allows e.g. to throw custom exceptions instead.
        /// </summary>
        /// <param name="interceptionCallback">The callback to execute before the change occurred.</param>
        /// <param name="predicate">
        ///     (optional) A predicate used to filter which callbacks should be intercepted.<br />
        ///     If set to <see langword="null" /> (default value) all callbacks are intercepted.
        /// </param>
        FileSystemMock Change(Action<ChangeDescription> interceptionCallback,
                              Func<ChangeDescription, bool>? predicate = null);
    }
}