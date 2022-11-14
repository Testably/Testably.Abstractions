using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.RandomSystem;

internal sealed class RandomFactoryMock : IRandomFactory
{
	private readonly MockRandomSystem _mockRandomSystem;

	internal RandomFactoryMock(MockRandomSystem randomSystem)
	{
		_mockRandomSystem = randomSystem;
	}

	#region IRandomFactory Members

	/// <inheritdoc cref="IRandomSystemExtensionPoint.RandomSystem" />
	public IRandomSystem RandomSystem => _mockRandomSystem;

	/// <inheritdoc cref="IRandomFactory.Shared" />
	public IRandom Shared
		=> _mockRandomSystem.RandomProvider.GetRandom();

	/// <inheritdoc cref="IRandomFactory.New()" />
	public IRandom New()
		=> _mockRandomSystem.RandomProvider.GetRandom(
			RandomProvider.NewSeed());

	/// <inheritdoc cref="IRandomFactory.New(int)" />
	public IRandom New(int seed)
		=> _mockRandomSystem.RandomProvider.GetRandom(seed);

	#endregion
}
