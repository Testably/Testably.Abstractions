namespace Testably.Abstractions;

/// <summary>
///     Allows abstracting random-related system dependencies.
/// </summary>
public partial interface IRandomSystem
{
    /// <summary>
    ///     Abstractions for <see cref="System.Guid" />.
    /// </summary>
    IGuid Guid { get; }

    /// <summary>
    ///     Abstractions for <see cref="System.Random" />.
    /// </summary>
    IRandomFactory Random { get; }

    /// <summary>
    ///     Interface to support implementing extension methods on top of nested <see cref="IRandomSystem" /> interfaces.
    /// </summary>
    interface IRandomSystemExtensionPoint
    {
        /// <summary>
        ///     Exposes the underlying random system implementation.
        ///     <para />
        ///     This is useful for implementing extension methods.
        /// </summary>
        IRandomSystem RandomSystem { get; }
    }
}