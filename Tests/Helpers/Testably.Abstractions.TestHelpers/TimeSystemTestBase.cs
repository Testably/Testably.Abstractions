using System;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     If referencing this base class, the source generator will automatically create two classes implementing your class:
///     <br />
///     - one will provide a `RealTimeSystem`<br />
///     - one will provide a `MockTimeSystem`<br />
///     Thus your tests run on both systems identically.
/// </summary>
/// <remarks>
///     Important: You have to mark your class as ´partial`!
/// </remarks>
public abstract class TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	public TTimeSystem TimeSystem { get; }

	protected TimeSystemTestBase(TTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	protected TimeSystemTestBase()
	{
		throw new NotSupportedException(
			"The SourceGenerator didn't create the corresponding files!");
	}
}
