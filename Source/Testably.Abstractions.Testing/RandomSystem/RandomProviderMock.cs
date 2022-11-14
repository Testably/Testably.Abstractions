using System;
using Testably.Abstractions.RandomSystem;
using static Testably.Abstractions.Testing.RandomProvider;

namespace Testably.Abstractions.Testing.RandomSystem;

internal sealed class RandomProviderMock : IRandomProvider
{
	private static Generator<Guid> DefaultGuidGenerator
		=> Generator<Guid>.FromCallback(Guid.NewGuid);

	private readonly Generator<Guid> _guidGenerator;
	private readonly Func<int, IRandom> _randomGenerator;

	public RandomProviderMock(
		Func<int, IRandom>? randomGenerator = null,
		Generator<Guid>? guidGenerator = null)
	{
		_guidGenerator = guidGenerator ?? DefaultGuidGenerator;
		_randomGenerator = randomGenerator ?? DefaultRandomGenerator;
	}

	#region IRandomProvider Members

	/// <inheritdoc cref="IRandomProvider.GetGuid" />
	public Guid GetGuid()
		=> _guidGenerator.GetNext();

	/// <inheritdoc cref="IRandomProvider.GetRandom(int)" />
	public IRandom GetRandom(int seed = SharedSeed)
		=> _randomGenerator(seed);

	#endregion

	private IRandom DefaultRandomGenerator(int seed)
		=> new RandomMock(seed: seed);
}
