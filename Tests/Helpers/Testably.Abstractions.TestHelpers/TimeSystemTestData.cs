namespace Testably.Abstractions.TestHelpers;

public class TimeSystemTestData(ITimeSystem timeSystem)
{
	/// <summary>
	///     The time system to test.
	/// </summary>
	public ITimeSystem TimeSystem { get; } = timeSystem;

	/// <inheritdoc />
	public override string ToString()
		=> TimeSystem.GetType().Name;
}
