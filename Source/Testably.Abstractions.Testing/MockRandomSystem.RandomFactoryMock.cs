namespace Testably.Abstractions.Testing;

public sealed partial class MockRandomSystem
{
	private sealed class RandomFactoryMock : IRandomFactory
	{
		private readonly MockRandomSystem _randomSystemMock;

		internal RandomFactoryMock(MockRandomSystem randomSystem)
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