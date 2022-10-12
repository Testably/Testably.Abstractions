using System;
using static Testably.Abstractions.Testing.RandomProvider;

namespace Testably.Abstractions.Testing;

public sealed partial class RandomSystemMock
{
	internal sealed class RandomProviderMock : IRandomProvider
	{
		private static Generator<Guid> DefaultGuidGenerator
			=> Generator<Guid>.FromCallback(System.Guid.NewGuid);

		private readonly Generator<Guid> _guidGenerator;
		private readonly Func<int, IRandomSystem.IRandom> _randomGenerator;

		public RandomProviderMock(
			Func<int, IRandomSystem.IRandom>? randomGenerator = null,
			Generator<Guid>? guidGenerator = null)
		{
			_guidGenerator = guidGenerator ?? DefaultGuidGenerator;
			_randomGenerator = randomGenerator ?? DefaultRandomGenerator;
		}

		#region IRandomProvider Members

		/// <inheritdoc cref="RandomSystemMock.IRandomProvider.GetGuid" />
		public Guid GetGuid()
			=> _guidGenerator.GetNext();

		/// <inheritdoc cref="RandomSystemMock.IRandomProvider.GetRandom(int)" />
		public IRandomSystem.IRandom GetRandom(int seed = SharedSeed)
			=> _randomGenerator(seed);

		#endregion

		private IRandomSystem.IRandom DefaultRandomGenerator(int seed)
			=> new RandomMock(seed: seed);
	}
}