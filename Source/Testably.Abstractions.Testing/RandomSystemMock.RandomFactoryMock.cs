namespace Testably.Abstractions.Testing;

public sealed partial class RandomSystemMock
{
	private sealed class RandomFactoryMock : IRandomFactory
	{
		private readonly RandomSystemMock _randomSystemMock;

		internal RandomFactoryMock(RandomSystemMock randomSystem)
		{
			_randomSystemMock = randomSystem;
		}

		#region IRandomFactory Members

		/// <inheritdoc cref="IRandomSystemExtensionPoint.RandomSystem" />
		public IRandomSystem RandomSystem => _randomSystemMock;

		/// <inheritdoc cref="IRandomFactory.Shared" />
		public IRandom Shared
			=> _randomSystemMock.RandomProvider.GetRandom();

		/// <inheritdoc cref="IRandomFactory.New()" />
		public IRandom New()
			=> _randomSystemMock.RandomProvider.GetRandom(
				Testing.RandomProvider.NewSeed());

		/// <inheritdoc cref="IRandomFactory.New(int)" />
		public IRandom New(int seed)
			=> _randomSystemMock.RandomProvider.GetRandom(seed);

		#endregion
	}
}