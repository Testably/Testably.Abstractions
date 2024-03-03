using System;

namespace Testably.Abstractions.Testing.Statistics;

public interface IStatisticsGate
{
	bool TryGetLock(out IDisposable release);
}
