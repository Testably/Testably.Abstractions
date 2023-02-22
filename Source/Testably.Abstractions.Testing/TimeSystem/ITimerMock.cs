using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

public interface ITimerMock : ITimer
{
	ITimerMock Wait(int executionCount = 1, int timeout = 10000, Action<ITimerMock>? callback = null);
}
