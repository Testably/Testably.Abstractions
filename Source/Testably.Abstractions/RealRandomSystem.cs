using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions;

/// <summary>
///     Default implementation for random-related system dependencies.
///     <para />
///     Implements <seealso cref="IRandomSystem" />
/// </summary>
public sealed class RealRandomSystem : IRandomSystem
{
	/// <summary>
	///     Initializes a new instance of <see cref="RealRandomSystem" />
	///     which wraps the random-related system dependencies from <see cref="IRandomSystem" />.
	/// </summary>
	public RealRandomSystem()
	{
		Guid = new GuidWrapper(this);
		Random = new RandomFactory(this);
	}

	#region IRandomSystem Members

	/// <inheritdoc cref="IRandomSystem.Guid" />
	public IGuid Guid { get; }

	/// <inheritdoc cref="IRandomSystem.Random" />
	public IRandomFactory Random { get; }

	#endregion
}
