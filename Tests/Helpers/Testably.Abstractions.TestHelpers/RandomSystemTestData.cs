namespace Testably.Abstractions.TestHelpers;

public class RandomSystemTestData(IRandomSystem randomSystem)
{
	/// <summary>
	///     The random system to test.
	/// </summary>
	public IRandomSystem RandomSystem { get; } = randomSystem;

	/// <inheritdoc />
	public override string ToString()
		=> RandomSystem.GetType().Name;
}
