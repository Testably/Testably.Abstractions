namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for manipulating the random system. Implements <see cref="IRandomSystem" />.
///     <para />
///     The <see cref="RandomProvider" /> allows manipulating the simulated random system.
/// </summary>
public sealed partial class RandomSystemMock : IRandomSystem
{
	/// <summary>
	///     The random provider for the currently simulated system.
	/// </summary>
	public IRandomProvider RandomProvider { get; }

	private readonly GuidMock _guidMock;
	private readonly RandomFactoryMock _randomFactoryMock;

	/// <summary>
	///     Initializes the <see cref="RandomSystemMock" /> with a fixed seed random.
	/// </summary>
	public RandomSystemMock() : this(Testing.RandomProvider.Default())
	{
	}

	/// <summary>
	///     Initializes the <see cref="RandomSystemMock" /> with the specified <paramref name="randomProviderProvider" />.
	/// </summary>
	public RandomSystemMock(IRandomProvider randomProviderProvider)
	{
		RandomProvider = randomProviderProvider;
		_guidMock = new GuidMock(this);
		_randomFactoryMock = new RandomFactoryMock(this);
	}

	#region IRandomSystem Members

	/// <inheritdoc cref="IRandomSystem.Guid" />
	public IRandomSystem.IGuid Guid
		=> _guidMock;

	/// <inheritdoc cref="IRandomSystem.Random" />
	public IRandomSystem.IRandomFactory Random
		=> _randomFactoryMock;

	#endregion
}