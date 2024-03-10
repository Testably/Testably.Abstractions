﻿using System.Collections.Generic;

namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     Contains statistical information about the mock usage.
/// </summary>
public interface IStatistics
{
	/// <summary>
	///     Lists all called mocked methods.
	/// </summary>
	IReadOnlyDictionary<int, MethodStatistic> Methods { get; }
}
