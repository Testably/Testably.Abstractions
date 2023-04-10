using System;
using Testably.Abstractions.RandomSystem;
using static Testably.Abstractions.Testing.RandomProvider;

namespace Testably.Abstractions.Testing.RandomSystem;

internal sealed class RandomProviderMock : IRandomProvider
{
	[ThreadStatic] private static IRandom? _shared;

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

	private static IRandom DefaultRandomGenerator(int seed)
	{
		if (seed == SharedSeed)
		{
			return _shared ??= new RandomMock(seed: SharedSeed);
		}

		return new RandomMock(seed: seed);
	}
}
