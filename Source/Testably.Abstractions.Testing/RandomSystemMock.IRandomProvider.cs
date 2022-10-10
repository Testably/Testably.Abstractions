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
		IRandomSystem.IRandom GetRandom(int seed);
	}
}