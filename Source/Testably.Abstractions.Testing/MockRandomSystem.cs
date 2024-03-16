using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.RandomSystem;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for manipulating the random system. Implements <see cref="IRandomSystem" />.
///     <para />
///     The <see cref="RandomProvider" /> allows manipulating the simulated random system.
/// </summary>
public sealed class MockRandomSystem : IRandomSystem
{
	/// <summary>
	///     The random provider for the currently simulated system.
	/// </summary>
	public IRandomProvider RandomProvider { get; }

	private readonly GuidMock _guidMock;
	private readonly RandomFactoryMock _randomFactoryMock;

	/// <summary>
	///     Initializes the <see cref="MockRandomSystem" /> with a fixed seed random.
	/// </summary>
	public MockRandomSystem() : this(Testing.RandomProvider.Default())
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockRandomSystem" /> with the specified <paramref name="randomProviderProvider" />.
	/// </summary>
	public MockRandomSystem(IRandomProvider randomProviderProvider)
	{
		RandomProvider = randomProviderProvider;
		_guidMock = new GuidMock(this);
		_randomFactoryMock = new RandomFactoryMock(this);
	}

	#region IRandomSystem Members

	/// <inheritdoc cref="IRandomSystem.Guid" />
	public IGuid Guid
		=> _guidMock;

	/// <inheritdoc cref="IRandomSystem.Random" />
	public IRandomFactory Random
		=> _randomFactoryMock;

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> "MockRandomSystem";
}
