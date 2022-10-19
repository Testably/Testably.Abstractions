# Examples
This is an overview of the provided examples for "Testably.Abstractions".

- **[Configuration](Configuration/README.md)**  
  This example illustrates how the testing libraries can be configured in unit tests.

- **[Thread-aware time provider](ThreadAwareTimeProvider/README.md)**  
  The default implementation of the `ITimeProvider` uses a `DateTime` property to simulate the current time, that is advanced with every `Thread.Sleep` or `Task.Delay` call.
  In a scenario with multiple threads running in parallel, these would each influence each other differently in the mocked instance than "in the real world".  
  This example illustrates how to implement a thread-aware time provider for such a scenario.
