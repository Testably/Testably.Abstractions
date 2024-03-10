using System;

namespace Testably.Abstractions.Testing.Statistics;

internal interface IStatisticsGate
{
	bool TryGetLock(out IDisposable release);
	int GetCounter();
}
