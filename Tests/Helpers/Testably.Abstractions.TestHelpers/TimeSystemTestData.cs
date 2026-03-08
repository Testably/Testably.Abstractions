using System;

namespace Testably.Abstractions.TestHelpers;

public class TimeSystemTestData(DateTime now, ITimeSystem timeSystem)
{
	/// <summary>
	///     The current date and time when creating the time system.
	/// </summary>
	public DateTime Now { get; } = now;

	/// <summary>
	///     The time system to test.
	/// </summary>
	public ITimeSystem TimeSystem { get; } = timeSystem;

	/// <inheritdoc />
	public override string ToString()
		=> TimeSystem.GetType().Name;
}
