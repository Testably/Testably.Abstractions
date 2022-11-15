using System;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     If referencing this base class, the source generator will automatically create two classes implementing your class:
///     <br />
///     - one will provide a `RealRandomSystem`<br />
///     - one will provide a `MockRandomSystem`<br />
///     Thus your tests run on both systems identically.
/// </summary>
/// <remarks>
///     Important: You have to mark your class as ´partial`!
/// </remarks>
public abstract class RandomSystemTestBase<TRandomSystem>
	where TRandomSystem : IRandomSystem
{
	public TRandomSystem RandomSystem { get; }

	protected RandomSystemTestBase(TRandomSystem randomSystem)
	{
		RandomSystem = randomSystem;
	}

	protected RandomSystemTestBase()
	{
		throw new NotSupportedException(
			"The SourceGenerator didn't create the corresponding files!");
	}
}
