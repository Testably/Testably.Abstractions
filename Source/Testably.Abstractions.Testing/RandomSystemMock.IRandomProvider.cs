using System;

namespace Testably.Abstractions.Testing;

public sealed partial class RandomSystemMock
{
	/// <summary>
	///     The random provider for the <see cref="RandomSystemMock" />
	/// </summary>
	public interface IRandomProvider
	{
		/// <summary>
		///     Creates the next <see cref="System.Guid" /> returned by <see cref="IRandomSystem.IGuid.NewGuid()" />
		/// </summary>
		Guid GetGuid();

		/// <summary>
		///     Creates the <see cref="IRandomSystem.IRandom" /> instance for the provided <paramref name="seed" />.
		/// </summary>
		/// <param name="seed">
		///     (optional) The seed used for the generated random instance.<br />
		///     Defaults to the <see cref="Testing.RandomProvider.SharedSeed" />.
		/// </param>
		IRandomSystem.IRandom GetRandom(int seed = Testing.RandomProvider.SharedSeed);
	}
}