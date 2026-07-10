using System;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Testing;

/// <summary>
///     <see cref="ITimeProvider" />s for use in the constructor of <see cref="MockTimeSystem" />.
/// </summary>
[Obsolete(
	"Renamed to `TimeProviderFactory` to avoid confusion with the BCL `System.TimeProvider`. This type will be removed in a future major version.")]
public static class TimeProvider
{
	/// <inheritdoc cref="TimeProviderFactory.Now()" />
	public static ITimeProviderFactory Now()
		=> TimeProviderFactory.Now();

	/// <inheritdoc cref="TimeProviderFactory.Random()" />
	public static ITimeProviderFactory Random()
		=> TimeProviderFactory.Random();

	/// <inheritdoc cref="TimeProviderFactory.Use(DateTime)" />
	public static ITimeProviderFactory Use(DateTime time)
		=> TimeProviderFactory.Use(time);
}
