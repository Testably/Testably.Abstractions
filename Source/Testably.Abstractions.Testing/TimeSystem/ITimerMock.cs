namespace Testably.Abstractions.Testing.TimeSystem;

public interface ITimerMock
{
	void Wait(int executionCount = 1, int timeout = 10000);
}
