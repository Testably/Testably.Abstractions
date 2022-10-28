using System;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.RandomSystem;

/// <summary>
///     The random provider for the <see cref="MockRandomSystem" />
/// </summary>
public interface IRandomProvider
{
	/// <summary>
	///     Creates the next <see cref="System.Guid" /> returned by <see cref="IGuid.NewGuid()" />
	/// </summary>
	Guid GetGuid();

	/// <summary>
	///     Creates the <see cref="IRandom" /> instance for the provided <paramref name="seed" />.
	/// </summary>
	/// <param name="seed">
	///     (optional) The seed used for the generated random instance.<br />
	///     Defaults to the <see cref="Testing.RandomProvider.SharedSeed" />.
	/// </param>
	IRandom GetRandom(int seed = RandomProvider.SharedSeed);
}