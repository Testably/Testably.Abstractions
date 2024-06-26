﻿namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     Contains statistical information about the mock usage.
/// </summary>
public interface IStatistics
{
	/// <summary>
	///     Lists all called mocked methods.
	/// </summary>
	MethodStatistic[] Methods { get; }

	/// <summary>
	///     Lists all accessed mocked properties.
	/// </summary>
	PropertyStatistic[] Properties { get; }
}

/// <summary>
///     Contains statistical information about the mock usage.
/// </summary>
// ReSharper disable once UnusedTypeParameter
public interface IStatistics<TType> : IStatistics
{
	// Empty wrapper interface to by type-safe.
}
