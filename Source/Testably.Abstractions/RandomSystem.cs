namespace Testably.Abstractions;

/// <summary>
///     Default implementation for random-related system dependencies.
///     <para />
///     Implements <seealso cref="IRandomSystem" />
/// </summary>
public sealed partial class RandomSystem : IRandomSystem
{
    #region IRandomSystem Members

    /// <inheritdoc cref="IRandomSystem.Guid" />
    public IRandomSystem.IGuid Guid
        => new GuidWrapper(this);

    /// <inheritdoc cref="IRandomSystem.Random" />
    public IRandomSystem.IRandomFactory Random
        => new RandomFactory(this);

    #endregion
}