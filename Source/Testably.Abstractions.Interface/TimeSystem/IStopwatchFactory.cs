using System.Diagnostics;
#if FEATURE_STOPWATCH_GETELAPSEDTIME
using System;
#endif

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Factory for abstracting creation of <see cref="Stopwatch" />.
/// </summary>
public interface IStopwatchFactory : ITimeSystemEntity
{
	/// <inheritdoc cref="Stopwatch.Frequency" />
	long Frequency { get; }

	/// <inheritdoc cref="Stopwatch.IsHighResolution" />
	bool IsHighResolution { get; }

	/// <inheritdoc cref="Stopwatch.GetTimestamp()" />
	long GetTimestamp();

	/// <inheritdoc cref="Stopwatch()" />
	IStopwatch New();

	/// <inheritdoc cref="Stopwatch.StartNew()" />
	IStopwatch StartNew();

	/// <summary>
	///     Wraps the <paramref name="stopwatch" /> to the testable <see cref="IStopwatch" />.
	/// </summary>
	IStopwatch Wrap(Stopwatch stopwatch);

#if FEATURE_STOPWATCH_GETELAPSEDTIME
	/// <inheritdoc cref="Stopwatch.GetElapsedTime(long)" />
	TimeSpan GetElapsedTime(long startingTimestamp);

	/// <inheritdoc cref="Stopwatch.GetElapsedTime(long, long)" />
	TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp);
#endif
}
