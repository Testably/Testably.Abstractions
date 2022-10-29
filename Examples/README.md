# Examples
This is an overview of the provided examples for "Testably.Abstractions".

- __[Configuration](Configuration/README.md)__  
  This example illustrates how the testing libraries can be configured in unit tests.

- __[Thread-aware time provider](ThreadAwareTimeProvider/README.md)__  
  The default implementation of the `ITimeProvider` uses a `DateTime` property to simulate the current time, that is advanced with every `Thread.Sleep` or `Task.Delay` call.
  In a scenario with multiple threads running in parallel, these would each influence each other differently in the mocked instance than "in the real world".  
  This example illustrates how to implement a thread-aware time provider for such a scenario.

- __[Zip-File](ZipFile/README.md)__  
  This example highlights how to use the `IFileSystem` to compress and uncompress directories using `System.IO.Compression`.
