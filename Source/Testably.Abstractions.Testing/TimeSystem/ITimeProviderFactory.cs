using System;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The factory for creating the time provider for the <see cref="MockTimeSystem" />
/// </summary>
public interface ITimeProviderFactory
{
	/// <summary>
	///     Creates a time provider for the <see cref="MockTimeSystem" />.
	/// </summary>
	/// <remarks>
	///     The <paramref name="onTimeChanged" /> callback is called whenever the time provider's time is changed. The callback
	///     receives the new time as a parameter.
	/// </remarks>
	ITimeProvider Create(Action<DateTime> onTimeChanged);
}
