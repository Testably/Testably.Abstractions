namespace Testably.Abstractions;

/// <summary>
///     Allows abstracting time-related system dependencies.
/// </summary>
public partial interface ITimeSystem
{
    /// <summary>
    ///     Abstractions for <see cref="System.DateTime" />.
    /// </summary>
    IDateTime DateTime { get; }

    /// <summary>
    ///     Abstractions for <see cref="System.Threading.Tasks.Task" />.
    /// </summary>
    ITask Task { get; }

    /// <summary>
    ///     Abstractions for <see cref="System.Threading.Thread" />.
    /// </summary>
    IThread Thread { get; }

    /// <summary>
    ///     Interface to support implementing extension methods on top of nested <see cref="ITimeSystem" /> interfaces.
    /// </summary>
    interface ITimeSystemExtensionPoint
    {
        /// <summary>
        ///     Exposes the underlying time system implementation.
        ///     <para />
        ///     This is useful for implementing extension methods.
        /// </summary>
        ITimeSystem TimeSystem { get; }
    }
}