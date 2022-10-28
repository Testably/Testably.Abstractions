using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The time provider for the <see cref="MockTimeSystem" />
/// </summary>
public interface ITimeProvider
{
	/// <summary>
	///     Gets or sets the <see cref="IDateTime.MaxValue" />
	/// </summary>
	DateTime MaxValue { get; set; }

	/// <summary>
	///     Gets or sets the <see cref="IDateTime.MinValue" />
	/// </summary>
	DateTime MinValue { get; set; }

	/// <summary>
	///     Gets or sets the <see cref="IDateTime.UnixEpoch" />
	/// </summary>
	DateTime UnixEpoch { get; set; }

	/// <summary>
	///     Advances the currently simulated system time by the <paramref name="interval" />
	/// </summary>
	/// <param name="interval"></param>
	void AdvanceBy(TimeSpan interval);

	/// <summary>
	///     Reads the currently simulated system time.
	/// </summary>
	DateTime Read();

	/// <summary>
	///     Sets the currently simulated system time to the specified <paramref name="value" />.
	/// </summary>
	void SetTo(DateTime value);
}