using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions;

/// <summary>
///     Allows abstracting random-related system dependencies.
/// </summary>
public interface IRandomSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.Guid" />.
	/// </summary>
	IGuid Guid { get; }

	/// <summary>
	///     Abstractions for <see cref="System.Random" />.
	/// </summary>
	IRandomFactory Random { get; }
}
