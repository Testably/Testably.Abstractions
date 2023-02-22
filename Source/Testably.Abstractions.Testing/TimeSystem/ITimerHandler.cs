namespace Testably.Abstractions.Testing.TimeSystem;

public interface ITimerHandler
{
	ITimerMock this[int index] { get; }
}
