using System;

namespace Testably.Abstractions.Testing.Statistics;

internal interface IStatisticsGate
{
	int GetCounter();
	bool TryGetLock(out IDisposable release);
}
