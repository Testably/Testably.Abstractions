# Thread-aware time provider example
The default implementation of the `ITimeProvider` in `Testably.Abstractions.Testing` uses a `DateTime` property to simulate the current time, that is advanced with every `Thread.Sleep` or `Task.Delay` call.
In a scenario with multiple threads running in parallel, these would each influence each other differently in the mocked instance than "in the real world":
Two thread running in parallel, each sleeping for 5s should only advance the total clock by 5s, but in the mocked instance they would advance the clock by 10s.

This example illustrates how to implement a thread-aware time provider for such a scenario:

The `ThreadAwareTimeProvider` stores the time in a `AsyncLocal` property and provides a `SynchronizeClock` method to synchronize the clock across multiple threads.
Corresponding unit tests show the correct behaviour.

In order to use this, you just have to inject the custom provider to the `TimeSystemMock`:
```csharp
    TimeSystemMock timeSystem = new(new ThreadAwareTimeProvider());
```